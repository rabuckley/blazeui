using BlazeUI.Headless.Components.Select;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Tests.Components.Select;

public class SelectTests : BunitContext
{
    private SelectContext CreateContext(bool open = false, string? selectedValue = null) => new()
    {
        Open = open,
        SelectedValue = selectedValue,
        TriggerId = "test-trigger",
        PopupId = "test-popup",
    };

    private SelectItemContext CreateItemContext(bool selected = false) => new()
    {
        Selected = selected,
    };

    [Fact]
    public void TriggerHasListboxPopupRole()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectTrigger>(tp => tp.AddChildContent("Select")));

        Assert.Equal("listbox", cut.Find("button").GetAttribute("aria-haspopup"));
    }

    [Fact]
    public void TriggerHasAriaExpandedFalseWhenClosed()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectTrigger>(tp => tp.AddChildContent("Select")));

        Assert.Equal("false", cut.Find("button").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void TriggerHasAriaExpandedTrueWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectTrigger>(tp => tp.AddChildContent("Select")));

        Assert.Equal("true", cut.Find("button").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void TriggerHasDataPopupOpenWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectTrigger>(tp => tp.AddChildContent("Select")));

        Assert.NotNull(cut.Find("[data-popup-open]"));
    }

    [Fact]
    public void TriggerHasNoDataPopupOpenWhenClosed()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectTrigger>(tp => tp.AddChildContent("Select")));

        Assert.Empty(cut.FindAll("[data-popup-open]"));
    }

    [Fact]
    public void TriggerDisabledHasDisabledAttribute()
    {
        var ctx = CreateContext();
        ctx.Disabled = true;
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectTrigger>(tp => tp.AddChildContent("Select")));

        Assert.True(cut.Find("button").HasAttribute("disabled"));
        Assert.NotNull(cut.Find("[data-disabled]"));
    }

    [Fact]
    public void TriggerReadOnlyHasDataReadonlyAttribute()
    {
        var ctx = CreateContext();
        ctx.ReadOnly = true;
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectTrigger>(tp => tp.AddChildContent("Select")));

        Assert.NotNull(cut.Find("[data-readonly]"));
    }

    [Fact]
    public void TriggerRequiredHasDataRequiredAttribute()
    {
        var ctx = CreateContext();
        ctx.Required = true;
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectTrigger>(tp => tp.AddChildContent("Select")));

        Assert.NotNull(cut.Find("[data-required]"));
    }

    [Fact]
    public void TriggerControlsPopupId()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectTrigger>(tp => tp.AddChildContent("Select")));

        Assert.Equal("test-popup", cut.Find("button").GetAttribute("aria-controls"));
    }

    [Fact]
    public void TriggerIdMatchesContext()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectTrigger>(tp => tp.AddChildContent("Select")));

        Assert.Equal("test-trigger", cut.Find("button").GetAttribute("id"));
    }

    [Fact]
    public void PopupDoesNotRenderWhenNeverOpened()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectPopup>(tp => tp.AddChildContent("items")));

        // SelectPopup only mounts after being opened at least once.
        Assert.Empty(cut.FindAll("div"));
    }

    [Fact]
    public void PopupRendersWhenOpen()
    {
        // data-open is not rendered by Blazor — it's set by JS after positioning.
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectPopup>(tp => tp.AddChildContent("items")));

        Assert.NotNull(cut.Find("div"));
        Assert.Empty(cut.FindAll("[data-open]"));
    }

    [Fact]
    public void PopupHasPopupId()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectPopup>(tp => tp.AddChildContent("items")));

        Assert.Equal("test-popup", cut.Find("div").GetAttribute("id"));
    }

    [Fact]
    public void ItemHasOptionRole()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectItem>(tp => tp
                .Add(c => c.Value, "apple")
                .AddChildContent("Apple")));

        Assert.Equal("option", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void ItemAriaSelectedWhenSelected()
    {
        var ctx = CreateContext(selectedValue: "apple");
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectItem>(tp => tp
                .Add(c => c.Value, "apple")
                .AddChildContent("Apple")));

        Assert.Equal("true", cut.Find("div").GetAttribute("aria-selected"));
        Assert.NotNull(cut.Find("[data-selected]"));
    }

    [Fact]
    public void ItemAriaSelectedFalseWhenNotSelected()
    {
        var ctx = CreateContext(selectedValue: "banana");
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectItem>(tp => tp
                .Add(c => c.Value, "apple")
                .AddChildContent("Apple")));

        Assert.Equal("false", cut.Find("div").GetAttribute("aria-selected"));
        Assert.Empty(cut.FindAll("[data-selected]"));
    }

    [Fact]
    public void ItemHasDataValueAttribute()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectItem>(tp => tp
                .Add(c => c.Value, "apple")
                .AddChildContent("Apple")));

        Assert.Equal("apple", cut.Find("div").GetAttribute("data-value"));
    }

    [Fact]
    public void DisabledItemHasAriaDisabledAndDataAttribute()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectItem>(tp => tp
                .Add(c => c.Value, "cherry")
                .Add(c => c.Disabled, true)
                .AddChildContent("Cherry")));

        Assert.Equal("true", cut.Find("div").GetAttribute("aria-disabled"));
        Assert.NotNull(cut.Find("[data-disabled]"));
    }

    [Fact]
    public void DisabledItemHasTabIndexMinusOne()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectItem>(tp => tp
                .Add(c => c.Value, "cherry")
                .Add(c => c.Disabled, true)
                .AddChildContent("Cherry")));

        Assert.Equal("-1", cut.Find("div").GetAttribute("tabindex"));
    }

    [Fact]
    public void SeparatorHasSeparatorRole()
    {
        var cut = Render<SelectSeparator>();
        Assert.Equal("separator", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void GroupHasGroupRole()
    {
        var cut = Render<SelectGroup>();
        Assert.Equal("group", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void ValueShowsPlaceholderWhenNoSelection()
    {
        var ctx = CreateContext();
        ctx.Placeholder = "Pick a fruit";
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectValue>());

        Assert.Contains("Pick a fruit", cut.Find("span").TextContent);
        Assert.NotNull(cut.Find("[data-placeholder]"));
    }

    [Fact]
    public void ValueShowsSelectedLabel()
    {
        var ctx = CreateContext(selectedValue: "apple");
        ctx.SelectedLabel = "Apple";
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectValue>());

        Assert.Contains("Apple", cut.Find("span").TextContent);
        Assert.Empty(cut.FindAll("[data-placeholder]"));
    }

    [Fact]
    public void ValueFallsBackToSelectedValueWhenNoLabel()
    {
        var ctx = CreateContext(selectedValue: "apple");
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectValue>());

        Assert.Contains("apple", cut.Find("span").TextContent);
    }

    [Fact]
    public void TriggerShowsDataPlaceholderWhenNothingSelected()
    {
        var ctx = CreateContext(selectedValue: null);
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectTrigger>(tp => tp.AddChildContent("Select")));

        Assert.NotNull(cut.Find("[data-placeholder]"));
    }

    [Fact]
    public void TriggerHidesDataPlaceholderWhenSelected()
    {
        var ctx = CreateContext(selectedValue: "apple");
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectTrigger>(tp => tp.AddChildContent("Select")));

        Assert.Empty(cut.FindAll("[data-placeholder]"));
    }

    // -- SelectItemIndicator --
    // SelectItemIndicator reads selected state from SelectItemContext (cascaded by SelectItem),
    // not from a Value parameter. Tests inject the context directly.

    [Fact]
    public void ItemIndicatorRendersWhenSelected()
    {
        var itemCtx = CreateItemContext(selected: true);
        var cut = Render<CascadingValue<SelectItemContext>>(p => p
            .Add(c => c.Value, itemCtx)
            .AddChildContent<SelectItemIndicator>(np => np
                .AddChildContent("✓")));

        Assert.Contains("✓", cut.Markup);
        Assert.NotNull(cut.Find("[data-selected]"));
    }

    [Fact]
    public void ItemIndicatorDoesNotRenderWhenNotSelected()
    {
        var itemCtx = CreateItemContext(selected: false);
        var cut = Render<CascadingValue<SelectItemContext>>(p => p
            .Add(c => c.Value, itemCtx)
            .AddChildContent<SelectItemIndicator>(np => np
                .AddChildContent("✓")));

        Assert.DoesNotContain("✓", cut.Markup);
    }

    [Fact]
    public void ItemIndicatorRendersWhenKeepMountedAndNotSelected()
    {
        var itemCtx = CreateItemContext(selected: false);
        var cut = Render<CascadingValue<SelectItemContext>>(p => p
            .Add(c => c.Value, itemCtx)
            .AddChildContent<SelectItemIndicator>(np => np
                .Add(c => c.KeepMounted, true)
                .AddChildContent("✓")));

        // Renders but data-selected is absent (no data-unchecked per Base UI spec).
        Assert.Contains("✓", cut.Markup);
        Assert.Empty(cut.FindAll("[data-selected]"));
    }

    [Fact]
    public void ItemIndicatorHasAriaHidden()
    {
        var itemCtx = CreateItemContext(selected: true);
        var cut = Render<CascadingValue<SelectItemContext>>(p => p
            .Add(c => c.Value, itemCtx)
            .AddChildContent<SelectItemIndicator>(np => np
                .AddChildContent("✓")));

        Assert.Equal("true", cut.Find("span").GetAttribute("aria-hidden"));
    }

    // -- SelectItemText --

    [Fact]
    public void ItemTextRendersDivByDefault()
    {
        var cut = Render<SelectItemText>(p => p.AddChildContent("Apple"));
        Assert.Equal("DIV", cut.Find("div").TagName);
        Assert.Contains("Apple", cut.Markup);
    }

    // -- SelectBackdrop --

    [Fact]
    public void BackdropHasDataOpenWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectBackdrop>());

        Assert.NotNull(cut.Find("[data-open]"));
        Assert.Empty(cut.FindAll("[data-closed]"));
    }

    [Fact]
    public void BackdropHasDataClosedWhenClosed()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectBackdrop>());

        Assert.NotNull(cut.Find("[data-closed]"));
        Assert.Empty(cut.FindAll("[data-open]"));
    }
}
