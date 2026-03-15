using BlazeUI.Headless.Components.Autocomplete;
using BlazeUI.Headless.Components.Combobox;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Tests.Components.Autocomplete;

public class AutocompleteTests : BunitContext
{
    // Most tests inject ComboboxContext directly, which AutocompleteRoot cascades.
    // This avoids JS interop setup while still verifying the full ARIA contract.
    private ComboboxContext CreateContext(
        bool open = false,
        string? inputValue = null,
        string? filterValue = null,
        bool disabled = false,
        string? inputId = "test-input",
        string? listId = "test-list",
        AutocompleteMode? autocompleteMode = null) => new()
    {
        Open = open,
        InputValue = inputValue,
        FilterValue = filterValue,
        Disabled = disabled,
        InputId = inputId ?? "test-input",
        ListId = listId ?? "test-list",
        PopupId = "test-popup",
        AutocompleteMode = autocompleteMode,
        SetOpen = _ => Task.CompletedTask,
        Close = () => Task.CompletedTask,
        SelectItem = (_, _) => Task.CompletedTask,
        SetInputValue = _ => Task.CompletedTask,
        RemoveValue = _ => Task.CompletedTask,
        ClearSelection = () => Task.CompletedTask,
    };

    // -- AutocompleteValue rendering --

    [Fact]
    public void Value_RendersInputValueByDefault()
    {
        var ctx = CreateContext(inputValue: "hel");

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<AutocompleteValue>());

        Assert.Contains("hel", cut.Markup);
    }

    [Fact]
    public void Value_RendersEmptyStringWhenNoInputValue()
    {
        var ctx = CreateContext(inputValue: null);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<AutocompleteValue>());

        // AutocompleteValue renders an empty string when no input value is set.
        // The markup should not contain any text node with a non-empty value.
        Assert.DoesNotContain("<div", cut.Markup);
        Assert.DoesNotContain("<span", cut.Markup);
    }

    [Fact]
    public void Value_FunctionChildReceivesCurrentValue()
    {
        var ctx = CreateContext(inputValue: "hel");

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<AutocompleteValue>(av => av
                .Add(v => v.ValueContent, (RenderFragment<string>)(val => builder =>
                {
                    builder.OpenElement(0, "span");
                    builder.AddAttribute(1, "data-testid", "value");
                    builder.AddContent(2, val);
                    builder.CloseElement();
                }))
            ));

        Assert.Equal("hel", cut.Find("[data-testid=value]").TextContent);
    }

    [Fact]
    public void Value_StaticChildOverridesValueDisplay()
    {
        var ctx = CreateContext(inputValue: "typed-text");

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<AutocompleteValue>(av => av
                .Add(v => v.ChildContent, (RenderFragment)(builder =>
                {
                    builder.OpenElement(0, "span");
                    builder.AddContent(1, "Custom Display Text");
                    builder.CloseElement();
                }))
            ));

        // Static children override the value — typed-text should not appear.
        Assert.Contains("Custom Display Text", cut.Markup);
        Assert.DoesNotContain("typed-text", cut.Markup);
    }

    [Fact]
    public void Value_FunctionChildReceivesEmptyStringWhenNoValue()
    {
        var ctx = CreateContext(inputValue: null);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<AutocompleteValue>(av => av
                .Add(v => v.ValueContent, (RenderFragment<string>)(val => builder =>
                {
                    builder.OpenElement(0, "span");
                    builder.AddAttribute(1, "data-testid", "value");
                    builder.AddContent(2, val == "" ? "empty" : val);
                    builder.CloseElement();
                }))
            ));

        Assert.Equal("empty", cut.Find("[data-testid=value]").TextContent);
    }

    // -- Input ARIA attributes for autocomplete modes --

    [Fact]
    public void Input_AriaAutocompleteIsListByDefault()
    {
        // When AutocompleteMode is not set (null), ComboboxInput defaults to "list".
        var ctx = CreateContext(autocompleteMode: null);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxInput>());

        Assert.Equal("list", cut.Find("input").GetAttribute("aria-autocomplete"));
    }

    [Fact]
    public void Input_AriaAutocompleteIsListWhenModeIsList()
    {
        var ctx = CreateContext(autocompleteMode: AutocompleteMode.List);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxInput>());

        Assert.Equal("list", cut.Find("input").GetAttribute("aria-autocomplete"));
    }

    [Fact]
    public void Input_AriaAutocompleteIsBothWhenModeIsBoth()
    {
        var ctx = CreateContext(autocompleteMode: AutocompleteMode.Both);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxInput>());

        Assert.Equal("both", cut.Find("input").GetAttribute("aria-autocomplete"));
    }

    [Fact]
    public void Input_AriaAutocompleteIsBothWhenModeIsInline()
    {
        var ctx = CreateContext(autocompleteMode: AutocompleteMode.Inline);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxInput>());

        Assert.Equal("both", cut.Find("input").GetAttribute("aria-autocomplete"));
    }

    [Fact]
    public void Input_AriaAutocompleteIsNoneWhenModeIsNone()
    {
        var ctx = CreateContext(autocompleteMode: AutocompleteMode.None);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxInput>());

        Assert.Equal("none", cut.Find("input").GetAttribute("aria-autocomplete"));
    }

    // -- Input ARIA attributes (combobox role / open state) --

    [Fact]
    public void Input_HasComboboxRole()
    {
        var ctx = CreateContext();

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxInput>());

        Assert.Equal("combobox", cut.Find("input").GetAttribute("role"));
    }

    [Fact]
    public void Input_AriaExpandedFalseWhenClosed()
    {
        var ctx = CreateContext(open: false);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxInput>());

        Assert.Equal("false", cut.Find("input").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void Input_AriaExpandedTrueWhenOpen()
    {
        var ctx = CreateContext(open: true);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxInput>());

        Assert.Equal("true", cut.Find("input").GetAttribute("aria-expanded"));
    }

    // -- Item filtering by Mode --

    [Fact]
    public void Item_VisibleWhenNoFilterInListMode()
    {
        // filterValue is null — no active filter, all items are visible.
        var ctx = CreateContext(open: true, filterValue: null, autocompleteMode: AutocompleteMode.List);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxItem>(ip => ip
                .Add(i => i.Value, "apple")));

        var item = cut.Find("[role=option]");
        Assert.False(item.HasAttribute("style") &&
            item.GetAttribute("style")!.Contains("display:none"));
    }

    [Fact]
    public void Item_HiddenByFilterInListMode()
    {
        var ctx = CreateContext(open: true, filterValue: "xyz", autocompleteMode: AutocompleteMode.List);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxItem>(ip => ip
                .Add(i => i.Value, "apple")));

        var item = cut.Find("[role=option]");
        Assert.Contains("display:none", item.GetAttribute("style"));
    }

    [Fact]
    public void Item_NotFilteredInNoneMode()
    {
        // In None mode, filterValue should be null (set by AutocompleteRoot) so items are never hidden.
        // We simulate this by passing filterValue: null, which is what AutocompleteRoot sets for None mode.
        var ctx = CreateContext(open: true, filterValue: null, autocompleteMode: AutocompleteMode.None);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxItem>(ip => ip
                .Add(i => i.Value, "apple")));

        var item = cut.Find("[role=option]");
        Assert.False(item.HasAttribute("style") &&
            item.GetAttribute("style")!.Contains("display:none"));
    }

    // -- Item data-label attribute --

    [Fact]
    public void Item_RendersDataLabelWithLabelValue()
    {
        var ctx = CreateContext(open: true);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxItem>(ip => ip
                .Add(i => i.Value, "us")
                .Add(i => i.Label, "United States")));

        var item = cut.Find("[role=option]");
        Assert.Equal("United States", item.GetAttribute("data-label"));
    }

    [Fact]
    public void Item_DataLabelFallsBackToValue()
    {
        var ctx = CreateContext(open: true);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxItem>(ip => ip
                .Add(i => i.Value, "apple")));

        var item = cut.Find("[role=option]");
        Assert.Equal("apple", item.GetAttribute("data-label"));
    }

    // -- InlineComplete flag propagation --

    [Fact]
    public void Context_InlineCompleteFalseForListMode()
    {
        var ctx = CreateContext(autocompleteMode: AutocompleteMode.List);
        Assert.False(ctx.InlineComplete);
    }

    [Fact]
    public void Context_InlineCompleteFalseForNoneMode()
    {
        var ctx = CreateContext(autocompleteMode: AutocompleteMode.None);
        Assert.False(ctx.InlineComplete);
    }

    // -- Disabled state --

    [Fact]
    public void Input_DisabledAttributeSetWhenDisabled()
    {
        var ctx = CreateContext(disabled: true);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxInput>());

        Assert.True(cut.Find("input").HasAttribute("disabled"));
    }

    [Fact]
    public void Input_DataDisabledSetWhenDisabled()
    {
        var ctx = CreateContext(disabled: true);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxInput>());

        Assert.NotNull(cut.Find("[data-disabled]"));
    }
}
