using BlazeUI.Headless.Components.Combobox;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Tests.Components.Combobox;

public class ComboboxTests : BunitContext
{
    private ComboboxContext CreateContext(
        bool open = false,
        string? filterValue = null,
        string? selectedValue = null,
        bool multiple = false,
        IReadOnlyList<string>? selectedValues = null,
        bool disabled = false,
        bool readOnly = false,
        string? inputId = "test-input",
        string? listId = "test-list",
        string? labelId = null,
        string? placeholder = null) => new()
    {
        Open = open,
        SelectedValue = selectedValue,
        FilterValue = filterValue,
        Multiple = multiple,
        SelectedValues = selectedValues ?? [],
        InputId = inputId ?? "test-input",
        ListId = listId ?? "test-list",
        PopupId = "test-popup",
        LabelId = labelId,
        Placeholder = placeholder,
        Disabled = disabled,
        ReadOnly = readOnly,
        SetOpen = _ => Task.CompletedTask,
        Close = () => Task.CompletedTask,
        SelectItem = (_, _) => Task.CompletedTask,
        SetInputValue = _ => Task.CompletedTask,
        RemoveValue = _ => Task.CompletedTask,
        ClearSelection = () => Task.CompletedTask,
    };

    // -- Filtering --

    [Fact]
    public void ItemVisibleWhenNoFilter()
    {
        var ctx = CreateContext(open: true);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxItem>(ip => ip
                .Add(i => i.Value, "apple")));

        var item = cut.Find("[role=option]");
        Assert.False(item.HasAttribute("style") &&
            item.GetAttribute("style")!.Contains("display:none"));
    }

    [Fact]
    public void ItemHiddenWhenFilterDoesNotMatch()
    {
        var ctx = CreateContext(open: true, filterValue: "xyz");

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxItem>(ip => ip
                .Add(i => i.Value, "apple")));

        var item = cut.Find("[role=option]");
        Assert.Contains("display:none", item.GetAttribute("style"));
    }

    [Fact]
    public void ItemVisibleWhenFilterMatchesValue()
    {
        var ctx = CreateContext(open: true, filterValue: "app");

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxItem>(ip => ip
                .Add(i => i.Value, "apple")));

        var item = cut.Find("[role=option]");
        Assert.False(item.HasAttribute("style") &&
            item.GetAttribute("style")!.Contains("display:none"));
    }

    [Fact]
    public void FilterIsCaseInsensitive()
    {
        var ctx = CreateContext(open: true, filterValue: "APP");

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxItem>(ip => ip
                .Add(i => i.Value, "apple")));

        var item = cut.Find("[role=option]");
        Assert.False(item.HasAttribute("style") &&
            item.GetAttribute("style")!.Contains("display:none"));
    }

    [Fact]
    public void FilterMatchesSubstring()
    {
        var ctx = CreateContext(open: true, filterValue: "pl");

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxItem>(ip => ip
                .Add(i => i.Value, "apple")));

        var item = cut.Find("[role=option]");
        Assert.False(item.HasAttribute("style") &&
            item.GetAttribute("style")!.Contains("display:none"));
    }

    [Fact]
    public void FilterUsesLabelOverValue()
    {
        // Label matches but Value doesn't.
        var ctx = CreateContext(open: true, filterValue: "Red");

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxItem>(ip => ip
                .Add(i => i.Value, "fruit-1")
                .Add(i => i.Label, "Red Apple")));

        var item = cut.Find("[role=option]");
        Assert.False(item.HasAttribute("style") &&
            item.GetAttribute("style")!.Contains("display:none"));
    }

    [Fact]
    public void FilterAgainstLabelHidesWhenNoMatch()
    {
        // Neither Label nor Value matches.
        var ctx = CreateContext(open: true, filterValue: "xyz");

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxItem>(ip => ip
                .Add(i => i.Value, "fruit-1")
                .Add(i => i.Label, "Red Apple")));

        var item = cut.Find("[role=option]");
        Assert.Contains("display:none", item.GetAttribute("style"));
    }

    [Fact]
    public void HiddenParameterForcesHideEvenWhenFilterMatches()
    {
        var ctx = CreateContext(open: true);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxItem>(ip => ip
                .Add(i => i.Value, "apple")
                .Add(i => i.Hidden, true)));

        var item = cut.Find("[role=option]");
        Assert.Contains("display:none", item.GetAttribute("style"));
    }

    // -- Input ARIA --

    [Fact]
    public void InputHasComboboxRole()
    {
        var ctx = CreateContext();

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxInput>());

        Assert.Equal("combobox", cut.Find("input").GetAttribute("role"));
    }

    [Fact]
    public void InputHasAriaExpandedFalseWhenClosed()
    {
        var ctx = CreateContext(open: false);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxInput>());

        Assert.Equal("false", cut.Find("input").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void InputHasAriaExpandedTrueWhenOpen()
    {
        var ctx = CreateContext(open: true);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxInput>());

        Assert.Equal("true", cut.Find("input").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void InputHasAriaControlsPointingToList()
    {
        var ctx = CreateContext(listId: "my-list");

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxInput>());

        Assert.Equal("my-list", cut.Find("input").GetAttribute("aria-controls"));
    }

    [Fact]
    public void InputHasAriaLabelledByWhenLabelRegistered()
    {
        var ctx = CreateContext(labelId: "my-label");

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxInput>());

        Assert.Equal("my-label", cut.Find("input").GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void InputDoesNotEmitAriaLabelledByWhenNoLabel()
    {
        var ctx = CreateContext(labelId: null);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxInput>());

        Assert.Null(cut.Find("input").GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void InputHasAriaReadonlyWhenReadOnly()
    {
        var ctx = CreateContext(readOnly: true);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxInput>());

        Assert.Equal("true", cut.Find("input").GetAttribute("aria-readonly"));
    }

    [Fact]
    public void InputHasAutocompleteOff()
    {
        var ctx = CreateContext();

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxInput>());

        Assert.Equal("off", cut.Find("input").GetAttribute("autocomplete"));
    }

    // -- Item attributes --

    [Fact]
    public void ItemHasOptionRole()
    {
        var ctx = CreateContext(open: true);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxItem>(ip => ip
                .Add(i => i.Value, "apple")));

        Assert.Equal("option", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void SelectedItemHasAriaSelectedTrue()
    {
        var ctx = CreateContext(open: true, selectedValue: "apple");

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxItem>(ip => ip
                .Add(i => i.Value, "apple")));

        var item = cut.Find("[role=option]");
        Assert.Equal("true", item.GetAttribute("aria-selected"));
        Assert.NotNull(cut.Find("[data-selected]"));
    }

    [Fact]
    public void UnselectedItemHasAriaSelectedFalse()
    {
        var ctx = CreateContext(open: true, selectedValue: "banana");

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxItem>(ip => ip
                .Add(i => i.Value, "apple")));

        Assert.Equal("false", cut.Find("[role=option]").GetAttribute("aria-selected"));
    }

    [Fact]
    public void DisabledItemHasAriaDisabled()
    {
        var ctx = CreateContext(open: true);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxItem>(ip => ip
                .Add(i => i.Value, "apple")
                .Add(i => i.Disabled, true)));

        var item = cut.Find("[role=option]");
        Assert.Equal("true", item.GetAttribute("aria-disabled"));
        Assert.NotNull(cut.Find("[data-disabled]"));
    }

    // -- Multiple selection --

    [Fact]
    public void ItemSelectedInMultipleMode()
    {
        var ctx = CreateContext(multiple: true, selectedValues: ["apple", "banana"]);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxItem>(ip => ip
                .Add(i => i.Value, "apple")));

        Assert.Equal("true", cut.Find("[role=option]").GetAttribute("aria-selected"));
    }

    [Fact]
    public void ItemNotSelectedWhenNotInMultipleSelectionList()
    {
        var ctx = CreateContext(multiple: true, selectedValues: ["banana"]);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxItem>(ip => ip
                .Add(i => i.Value, "apple")));

        Assert.Equal("false", cut.Find("[role=option]").GetAttribute("aria-selected"));
    }

    [Fact]
    public void ListHasAriaMultiselectableWhenMultiple()
    {
        var ctx = CreateContext(multiple: true);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxList>());

        Assert.Equal("true", cut.Find("[role=listbox]").GetAttribute("aria-multiselectable"));
    }

    [Fact]
    public void ListDoesNotHaveAriaMultiselectableInSingleMode()
    {
        var ctx = CreateContext(multiple: false);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxList>());

        Assert.Null(cut.Find("[role=listbox]").GetAttribute("aria-multiselectable"));
    }

    // -- ComboboxLabel --

    [Fact]
    public void LabelRendersLabelElement()
    {
        var ctx = CreateContext();

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxLabel>(lp => lp
                .AddChildContent("Country")));

        var label = cut.Find("label");
        Assert.Equal("Country", label.TextContent);
    }

    [Fact]
    public void LabelHasForAttributePointingToInput()
    {
        var ctx = CreateContext(inputId: "the-input");

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxLabel>());

        Assert.Equal("the-input", cut.Find("label").GetAttribute("for"));
    }

    [Fact]
    public void LabelRegistersItsIdOnContext()
    {
        var ctx = CreateContext();

        // Rendering the Label should populate context.LabelId.
        Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxLabel>());

        Assert.NotNull(ctx.LabelId);
    }

    // -- ComboboxValue --

    [Fact]
    public void ValueShowsSelectedLabel()
    {
        var ctx = CreateContext(selectedValue: "apple");
        ctx.SelectedLabel = "Apple";

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxValue>());

        Assert.Contains("Apple", cut.Markup);
    }

    [Fact]
    public void ValueShowsPlaceholderWhenNothingSelected()
    {
        var ctx = CreateContext(placeholder: "Select a fruit");

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxValue>());

        Assert.Contains("Select a fruit", cut.Markup);
    }

    [Fact]
    public void ValueHasDataPlaceholderWhenNothingSelected()
    {
        var ctx = CreateContext(placeholder: "Pick one");

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxValue>());

        Assert.NotNull(cut.Find("[data-placeholder]"));
    }

    [Fact]
    public void ValueDoesNotHaveDataPlaceholderWhenSelected()
    {
        var ctx = CreateContext(selectedValue: "apple");

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxValue>());

        Assert.Empty(cut.FindAll("[data-placeholder]"));
    }

    // -- ComboboxTrigger --

    [Fact]
    public void TriggerHasAriaExpandedFalseWhenClosed()
    {
        var ctx = CreateContext(open: false);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxTrigger>());

        Assert.Equal("false", cut.Find("button").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void TriggerHasAriaExpandedTrueWhenOpen()
    {
        var ctx = CreateContext(open: true);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxTrigger>());

        Assert.Equal("true", cut.Find("button").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void TriggerHasAriaHaspopupListbox()
    {
        var ctx = CreateContext();

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxTrigger>());

        Assert.Equal("listbox", cut.Find("button").GetAttribute("aria-haspopup"));
    }

    [Fact]
    public void TriggerDoesNotHaveHardcodedAriaLabel()
    {
        // Base UI does not emit a hardcoded aria-label on the trigger.
        var ctx = CreateContext();

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxTrigger>());

        Assert.Null(cut.Find("button").GetAttribute("aria-label"));
    }

    [Fact]
    public void TriggerHasTabIndexMinusOne()
    {
        var ctx = CreateContext();

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxTrigger>());

        Assert.Equal("-1", cut.Find("button").GetAttribute("tabindex"));
    }

    // -- ComboboxIcon --

    [Fact]
    public void IconIsAriaHidden()
    {
        var ctx = CreateContext();

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxIcon>());

        Assert.Equal("true", cut.Find("span").GetAttribute("aria-hidden"));
    }

    // -- ComboboxStatus --

    [Fact]
    public void StatusHasRoleStatusAndAriaLive()
    {
        var ctx = CreateContext();

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxStatus>(sp => sp
                .AddChildContent("Loading…")));

        var el = cut.Find("[role=status]");
        Assert.Equal("polite", el.GetAttribute("aria-live"));
        Assert.Equal("true", el.GetAttribute("aria-atomic"));
    }

    // -- ComboboxInputGroup --

    [Fact]
    public void InputGroupHasRoleGroupAndDataSlot()
    {
        var ctx = CreateContext();

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxInputGroup>());

        var el = cut.Find("[role=group]");
        Assert.Equal("input-group", el.GetAttribute("data-slot"));
    }

    // -- ComboboxGroup + ComboboxGroupLabel --

    [Fact]
    public void GroupHasRoleGroup()
    {
        var cut = Render<ComboboxGroup>();

        Assert.Equal("group", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void GroupGetsAriaLabelledByFromGroupLabel()
    {
        // Arrange
        var cut = Render<ComboboxGroup>(p => p.AddChildContent<ComboboxGroupLabel>(lp =>
            lp.AddChildContent("Fruits")));

        // The group should pick up aria-labelledby pointing at the label's generated ID.
        var group = cut.Find("[role=group]");
        var label = cut.Find("div:not([role=group])");
        Assert.Equal(label.Id, group.GetAttribute("aria-labelledby"));
    }

    // -- ComboboxEmpty (previously NoResults) --

    [Fact]
    public void NoResultsHasStatusRole()
    {
        var ctx = CreateContext(open: true);
        ctx.RegisteredItems["id1"] = "apple";

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxEmpty>(np => np
                .AddChildContent("No items found.")));

        var el = cut.Find("[role=status]");
        Assert.Equal("polite", el.GetAttribute("aria-live"));
        Assert.Equal("true", el.GetAttribute("aria-atomic"));
    }

    [Fact]
    public void NoResultsRendersChildrenWhenEmpty()
    {
        var ctx = CreateContext(open: true, filterValue: "zzz");
        ctx.RegisteredItems["id1"] = "apple";

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxEmpty>(np => np
                .AddChildContent("No items found.")));

        Assert.Contains("No items found.", cut.Markup);
    }

    [Fact]
    public void NoResultsHidesChildrenWhenItemsMatch()
    {
        var ctx = CreateContext(open: true, filterValue: "app");
        ctx.RegisteredItems["id1"] = "apple";

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxEmpty>(np => np
                .AddChildContent("No items found.")));

        Assert.DoesNotContain("No items found.", cut.Markup);
    }

    [Fact]
    public void NoResultsHidesChildrenWhenNoFilter()
    {
        var ctx = CreateContext(open: true);
        ctx.RegisteredItems["id1"] = "apple";

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxEmpty>(np => np
                .AddChildContent("No items found.")));

        Assert.DoesNotContain("No items found.", cut.Markup);
    }

    // -- ComboboxItemIndicator --

    [Fact]
    public void ItemIndicatorRendersWhenItemSelected_ContextualPattern()
    {
        // Nested inside a ComboboxItem — no explicit Value parameter needed.
        var ctx = CreateContext(selectedValue: "apple");

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxItem>(ip => ip
                .Add(i => i.Value, "apple")
                .AddChildContent<ComboboxItemIndicator>(indp => indp
                    .AddChildContent("✓"))));

        Assert.Contains("✓", cut.Markup);
    }

    [Fact]
    public void ItemIndicatorHidesWhenItemNotSelected_ContextualPattern()
    {
        var ctx = CreateContext(selectedValue: "banana");

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxItem>(ip => ip
                .Add(i => i.Value, "apple")
                .AddChildContent<ComboboxItemIndicator>(indp => indp
                    .AddChildContent("✓"))));

        Assert.DoesNotContain("✓", cut.Markup);
    }

    [Fact]
    public void ItemIndicatorIsAriaHiddenWhenVisible()
    {
        var ctx = CreateContext(selectedValue: "apple");

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxItem>(ip => ip
                .Add(i => i.Value, "apple")
                .AddChildContent<ComboboxItemIndicator>(indp => indp
                    .AddChildContent("✓"))));

        Assert.Equal("true", cut.Find("span").GetAttribute("aria-hidden"));
    }

    // -- ComboboxClear --

    [Fact]
    public void ClearRendersButton()
    {
        var ctx = CreateContext(selectedValue: "apple");

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxClear>(cp => cp
                .AddChildContent("×")));

        Assert.NotNull(cut.Find("button"));
    }

    [Fact]
    public void ClearButtonHasTabIndexMinusOne()
    {
        var ctx = CreateContext();

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxClear>());

        Assert.Equal("-1", cut.Find("button").GetAttribute("tabindex"));
    }

    [Fact]
    public void ClearButtonIsDisabledWhenContextDisabled()
    {
        var ctx = CreateContext(disabled: true);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxClear>());

        Assert.True(cut.Find("button").HasAttribute("disabled"));
    }

    // -- ComboboxRow --

    [Fact]
    public void RowHasRoleRow()
    {
        var cut = Render<ComboboxRow>();

        Assert.Equal("row", cut.Find("div").GetAttribute("role"));
    }

    // -- ComboboxChips --

    [Fact]
    public void ChipsHasRoleToolbarWhenChipsPresent()
    {
        var ctx = CreateContext(multiple: true, selectedValues: ["apple", "banana"]);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxChips>());

        Assert.Equal("toolbar", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void ChipsDoesNotHaveRoleToolbarWhenNoChips()
    {
        var ctx = CreateContext(multiple: true, selectedValues: []);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxChips>());

        Assert.Null(cut.Find("div").GetAttribute("role"));
    }

    // -- ComboboxChip + ComboboxChipRemove --

    [Fact]
    public void ChipHasTabIndexMinusOne()
    {
        var ctx = CreateContext(multiple: true, selectedValues: ["apple"]);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxChip>(cp => cp
                .Add(c => c.Value, "apple")));

        Assert.Equal("-1", cut.Find("div").GetAttribute("tabindex"));
    }

    [Fact]
    public void ChipRemoveButtonHasTabIndexMinusOne()
    {
        var ctx = CreateContext(multiple: true, selectedValues: ["apple"]);

        var cut = Render<CascadingValue<ComboboxContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ComboboxChip>(cp => cp
                .Add(c => c.Value, "apple")
                .AddChildContent<ComboboxChipRemove>(rp => rp
                    .AddChildContent("×"))));

        Assert.Equal("-1", cut.Find("button").GetAttribute("tabindex"));
    }

    // -- Context.IsEmpty --

    [Fact]
    public void IsEmptyTrueWhenFilterMatchesNothing()
    {
        var ctx = CreateContext(filterValue: "xyz");
        ctx.RegisteredItems["id1"] = "apple";
        ctx.RegisteredItems["id2"] = "banana";

        Assert.True(ctx.IsEmpty);
    }

    [Fact]
    public void IsEmptyFalseWhenFilterMatchesSomething()
    {
        var ctx = CreateContext(filterValue: "app");
        ctx.RegisteredItems["id1"] = "apple";
        ctx.RegisteredItems["id2"] = "banana";

        Assert.False(ctx.IsEmpty);
    }

    [Fact]
    public void IsEmptyFalseWhenNoFilter()
    {
        var ctx = CreateContext();
        ctx.RegisteredItems["id1"] = "apple";

        Assert.False(ctx.IsEmpty);
    }
}
