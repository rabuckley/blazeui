using BlazeUI.Headless.Components.ContextMenu;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Tests.Components.ContextMenu;

public class ContextMenuTests : BunitContext
{
    public ContextMenuTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private ContextMenuContext CreateContext(bool open = false, bool disabled = false) => new()
    {
        Open = open,
        Disabled = disabled,
        PopupId = "test-popup",
        PositionerId = "test-positioner",
    };

    // -- Group --

    [Fact]
    public void GroupHasGroupRole()
    {
        var cut = Render<ContextMenuGroup>();
        Assert.Equal("group", cut.Find("div").GetAttribute("role"));
    }

    // -- GroupLabel --

    [Fact]
    public void GroupLabelRendersDiv()
    {
        var cut = Render<ContextMenuGroupLabel>(p => p.AddChildContent("Actions"));
        Assert.Equal("DIV", cut.Find("div").TagName);
        Assert.Contains("Actions", cut.Markup);
    }

    // -- Arrow --

    [Fact]
    public void ArrowRendersDiv()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuArrow>());

        Assert.Equal("DIV", cut.Find("div").TagName);
    }

    // -- Backdrop --

    [Fact]
    public void BackdropHasDataOpenWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuBackdrop>());

        Assert.NotNull(cut.Find("[data-open]"));
        Assert.Empty(cut.FindAll("[data-closed]"));
    }

    [Fact]
    public void BackdropHasDataClosedWhenClosed()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuBackdrop>());

        Assert.NotNull(cut.Find("[data-closed]"));
        Assert.Empty(cut.FindAll("[data-open]"));
    }

    // -- Trigger --

    [Fact]
    public void TriggerRendersDiv()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuTrigger>(np => np.AddChildContent("Area")));

        Assert.Equal("DIV", cut.Find("div").TagName);
    }

    [Fact]
    public void TriggerHasDataPopupOpenWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuTrigger>(np => np.AddChildContent("Right-click me")));

        Assert.NotNull(cut.Find("[data-popup-open]"));
    }

    [Fact]
    public void TriggerHasNoDataPopupOpenWhenClosed()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuTrigger>(np => np.AddChildContent("Right-click me")));

        Assert.Empty(cut.FindAll("[data-popup-open]"));
    }

    // -- LinkItem --

    [Fact]
    public void LinkItemRendersAnchor()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuLinkItem>(np => np.AddChildContent("Link")));

        Assert.Equal("A", cut.Find("a").TagName);
        Assert.Equal("menuitem", cut.Find("a").GetAttribute("role"));
    }

    [Fact]
    public void LinkItemDisabledHasAriaDisabled()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuLinkItem>(np => np
                .Add(c => c.Disabled, true)
                .AddChildContent("Link")));

        Assert.Equal("true", cut.Find("a").GetAttribute("aria-disabled"));
        Assert.NotNull(cut.Find("[data-disabled]"));
    }

    // -- CheckboxItem --

    [Fact]
    public void CheckboxItemHasMenuitemcheckboxRole()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuCheckboxItem>(np => np
                .Add(c => c.Checked, false)
                .AddChildContent("Toggle")));

        Assert.Equal("menuitemcheckbox", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void CheckboxItemAriaCheckedReflectsState()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuCheckboxItem>(np => np
                .Add(c => c.Checked, true)
                .AddChildContent("Toggle")));

        Assert.Equal("true", cut.Find("div").GetAttribute("aria-checked"));
        Assert.NotNull(cut.Find("[data-checked]"));
    }

    [Fact]
    public void CheckboxItemUncheckedHasDataUnchecked()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuCheckboxItem>(np => np
                .Add(c => c.Checked, false)
                .AddChildContent("Toggle")));

        Assert.Equal("false", cut.Find("div").GetAttribute("aria-checked"));
        Assert.NotNull(cut.Find("[data-unchecked]"));
    }

    [Fact]
    public void CheckboxItemHighlightedWhenIdMatches()
    {
        // Arrange: pre-set HighlightedItemId to the item's element id.
        // Use a fixed Id via the Id parameter to make assertion deterministic.
        var ctx = CreateContext();
        ctx.HighlightedItemId = "cb-item-1";

        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuCheckboxItem>(np => np
                .Add(c => c.Id, "cb-item-1")
                .Add(c => c.Checked, false)
                .AddChildContent("Toggle")));

        Assert.NotNull(cut.Find("[data-highlighted]"));
    }

    [Fact]
    public void CheckboxItemNotHighlightedWhenIdDoesNotMatch()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuCheckboxItem>(np => np
                .Add(c => c.Checked, false)
                .AddChildContent("Toggle")));

        Assert.Empty(cut.FindAll("[data-highlighted]"));
    }

    // -- CheckboxItemIndicator --

    [Fact]
    public void CheckboxItemIndicatorRendersWhenChecked()
    {
        var cut = Render<ContextMenuCheckboxItemIndicator>(p => p
            .Add(c => c.Checked, true)
            .AddChildContent("✓"));

        Assert.Contains("✓", cut.Markup);
        Assert.NotNull(cut.Find("[data-checked]"));
    }

    [Fact]
    public void CheckboxItemIndicatorHiddenWhenUncheckedAndNotKeepMounted()
    {
        var cut = Render<ContextMenuCheckboxItemIndicator>(p => p
            .Add(c => c.Checked, false)
            .AddChildContent("✓"));

        Assert.DoesNotContain("✓", cut.Markup);
    }

    [Fact]
    public void CheckboxItemIndicatorVisibleWhenKeepMounted()
    {
        var cut = Render<ContextMenuCheckboxItemIndicator>(p => p
            .Add(c => c.Checked, false)
            .Add(c => c.KeepMounted, true)
            .AddChildContent("✓"));

        Assert.Contains("✓", cut.Markup);
        Assert.NotNull(cut.Find("[data-unchecked]"));
    }

    // -- RadioGroup --

    [Fact]
    public void RadioGroupHasGroupRole()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuRadioGroup>(np => np
                .Add(c => c.Value, "opt1")));

        Assert.Equal("group", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void RadioGroupSetsContextRadioValue()
    {
        var ctx = CreateContext();
        Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuRadioGroup>(np => np
                .Add(c => c.Value, "opt1")));

        Assert.Equal("opt1", ctx.RadioGroupValue);
    }

    // -- RadioItem --

    [Fact]
    public void RadioItemHasMenuitemradioRole()
    {
        var ctx = CreateContext();
        ctx.RadioGroupValue = "opt1";
        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuRadioItem>(np => np
                .Add(c => c.Value, "opt1")
                .AddChildContent("Option 1")));

        Assert.Equal("menuitemradio", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void RadioItemCheckedWhenMatchesGroupValue()
    {
        var ctx = CreateContext();
        ctx.RadioGroupValue = "opt1";
        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuRadioItem>(np => np
                .Add(c => c.Value, "opt1")
                .AddChildContent("Option 1")));

        Assert.Equal("true", cut.Find("div").GetAttribute("aria-checked"));
        Assert.NotNull(cut.Find("[data-checked]"));
    }

    [Fact]
    public void RadioItemUncheckedWhenDoesNotMatchGroupValue()
    {
        var ctx = CreateContext();
        ctx.RadioGroupValue = "opt1";
        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuRadioItem>(np => np
                .Add(c => c.Value, "opt2")
                .AddChildContent("Option 2")));

        Assert.Equal("false", cut.Find("div").GetAttribute("aria-checked"));
        Assert.NotNull(cut.Find("[data-unchecked]"));
    }

    [Fact]
    public void RadioItemHighlightedWhenIdMatches()
    {
        var ctx = CreateContext();
        ctx.RadioGroupValue = "opt1";
        ctx.HighlightedItemId = "radio-item-1";

        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuRadioItem>(np => np
                .Add(c => c.Id, "radio-item-1")
                .Add(c => c.Value, "opt1")
                .AddChildContent("Option 1")));

        Assert.NotNull(cut.Find("[data-highlighted]"));
    }

    [Fact]
    public void RadioItemNotHighlightedWhenIdDoesNotMatch()
    {
        var ctx = CreateContext();
        ctx.RadioGroupValue = "opt1";
        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuRadioItem>(np => np
                .Add(c => c.Value, "opt1")
                .AddChildContent("Option 1")));

        Assert.Empty(cut.FindAll("[data-highlighted]"));
    }

    // -- RadioItemIndicator --

    [Fact]
    public void RadioItemIndicatorRendersWhenChecked()
    {
        var ctx = CreateContext();
        ctx.RadioGroupValue = "opt1";
        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuRadioItemIndicator>(np => np
                .Add(c => c.Value, "opt1")
                .AddChildContent("●")));

        Assert.Contains("●", cut.Markup);
        Assert.NotNull(cut.Find("[data-checked]"));
    }

    [Fact]
    public void RadioItemIndicatorHiddenWhenNotChecked()
    {
        var ctx = CreateContext();
        ctx.RadioGroupValue = "opt1";
        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuRadioItemIndicator>(np => np
                .Add(c => c.Value, "opt2")
                .AddChildContent("●")));

        Assert.DoesNotContain("●", cut.Markup);
    }

    // -- Item --

    [Fact]
    public void ItemHasMenuitemRole()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuItem>(np => np.AddChildContent("Cut")));

        Assert.Equal("menuitem", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void ItemDisabledHasAriaDisabled()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuItem>(np => np
                .Add(c => c.Disabled, true)
                .AddChildContent("Cut")));

        Assert.Equal("true", cut.Find("div").GetAttribute("aria-disabled"));
        Assert.Equal("-1", cut.Find("div").GetAttribute("tabindex"));
    }

    // -- Separator --

    [Fact]
    public void SeparatorHasSeparatorRole()
    {
        var cut = Render<ContextMenuSeparator>();
        Assert.Equal("separator", cut.Find("div").GetAttribute("role"));
    }

    // -- SubmenuTrigger --

    [Fact]
    public void SubmenuTriggerHasMenuitemRole()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuSubmenuTrigger>(np => np.AddChildContent("More")));

        Assert.Equal("menuitem", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void SubmenuTriggerHasAriaHasPopupMenu()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuSubmenuTrigger>(np => np.AddChildContent("More")));

        Assert.Equal("menu", cut.Find("div").GetAttribute("aria-haspopup"));
    }

    [Fact]
    public void SubmenuTriggerAriaExpandedReflectsOpenState()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuSubmenuTrigger>(np => np.AddChildContent("More")));

        Assert.Equal("true", cut.Find("div").GetAttribute("aria-expanded"));
        Assert.NotNull(cut.Find("[data-popup-open]"));
    }

    [Fact]
    public void SubmenuTriggerDisabledHasDataDisabledAndAriaDisabled()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<ContextMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ContextMenuSubmenuTrigger>(np => np
                .Add(c => c.Disabled, true)
                .AddChildContent("More")));

        Assert.NotNull(cut.Find("[data-disabled]"));
        Assert.Equal("true", cut.Find("div").GetAttribute("aria-disabled"));
        Assert.Equal("-1", cut.Find("div").GetAttribute("tabindex"));
    }
}
