using BlazeUI.Headless.Components.Menu;
using BlazeUI.Headless.Core;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Headless.Tests.Components.Menu;

public class MenuTests : BunitContext
{
    public MenuTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazeUI();
    }

    private MenuContext CreateContext(bool open = false) => new()
    {
        Open = open,
        TriggerId = "test-trigger",
        PopupId = "test-popup",
    };

    // -- MenuTrigger --

    [Fact]
    public void Trigger_RendersButton()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuTrigger>(np => np.AddChildContent("Open")));

        Assert.Equal("BUTTON", cut.Find("button").TagName);
    }

    [Fact]
    public void Trigger_HasAriaHaspopupMenu()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuTrigger>(np => np.AddChildContent("Open")));

        Assert.Equal("menu", cut.Find("button").GetAttribute("aria-haspopup"));
    }

    [Fact]
    public void Trigger_AriaExpandedFalseWhenClosed()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuTrigger>(np => np.AddChildContent("Open")));

        Assert.Equal("false", cut.Find("button").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void Trigger_AriaExpandedTrueWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuTrigger>(np => np.AddChildContent("Open")));

        Assert.Equal("true", cut.Find("button").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void Trigger_HasAriaControlsPointingToPopup()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuTrigger>(np => np.AddChildContent("Open")));

        Assert.Equal("test-popup", cut.Find("button").GetAttribute("aria-controls"));
    }

    [Fact]
    public void Trigger_HasDataPopupOpenWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuTrigger>(np => np.AddChildContent("Open")));

        Assert.NotNull(cut.Find("[data-popup-open]"));
    }

    [Fact]
    public void Trigger_NoDataPopupOpenWhenClosed()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuTrigger>(np => np.AddChildContent("Open")));

        Assert.Empty(cut.FindAll("[data-popup-open]"));
    }

    // -- MenuItem --

    [Fact]
    public void Item_HasMenuitemRole()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuItem>(np => np.AddChildContent("Cut")));

        Assert.Equal("menuitem", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void Item_DisabledHasAriaDisabledAndDataDisabled()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuItem>(np => np
                .Add(c => c.Disabled, true)
                .AddChildContent("Cut")));

        var div = cut.Find("div");
        Assert.Equal("true", div.GetAttribute("aria-disabled"));
        Assert.Equal("-1", div.GetAttribute("tabindex"));
        Assert.NotNull(cut.Find("[data-disabled]"));
    }

    [Fact]
    public void Item_HighlightedHasDataHighlighted()
    {
        var ctx = CreateContext();
        ctx.HighlightedItemId = "highlight-me";

        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuItem>(np => np
                .Add(c => c.Id, "highlight-me")
                .AddChildContent("Cut")));

        Assert.NotNull(cut.Find("[data-highlighted]"));
    }

    // -- MenuSeparator --

    [Fact]
    public void Separator_HasSeparatorRole()
    {
        var cut = Render<MenuSeparator>();
        Assert.Equal("separator", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void Separator_HasAriaOrientationHorizontal()
    {
        var cut = Render<MenuSeparator>();
        Assert.Equal("horizontal", cut.Find("div").GetAttribute("aria-orientation"));
    }

    // -- MenuBackdrop --

    [Fact]
    public void Backdrop_HasRolePresentation()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuBackdrop>());

        Assert.Equal("presentation", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void Backdrop_HasDataOpenWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuBackdrop>());

        Assert.NotNull(cut.Find("[data-open]"));
        Assert.Empty(cut.FindAll("[data-closed]"));
    }

    [Fact]
    public void Backdrop_HasDataClosedWhenClosed()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuBackdrop>());

        Assert.NotNull(cut.Find("[data-closed]"));
        Assert.Empty(cut.FindAll("[data-open]"));
    }

    // -- MenuArrow --

    [Fact]
    public void Arrow_HasAriaHidden()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuArrow>());

        Assert.Equal("true", cut.Find("div").GetAttribute("aria-hidden"));
    }

    [Fact]
    public void Arrow_HasDataSideFromContext()
    {
        var ctx = CreateContext(open: true);
        ctx.PlacementSide = "bottom";
        ctx.PlacementAlign = "start";

        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuArrow>());

        Assert.Equal("bottom", cut.Find("div").GetAttribute("data-side"));
        Assert.Equal("start", cut.Find("div").GetAttribute("data-align"));
    }

    // -- MenuGroup --

    [Fact]
    public void Group_HasGroupRole()
    {
        var cut = Render<MenuGroup>();
        Assert.Equal("group", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void Group_AriaLabelledByLinkedToGroupLabel()
    {
        // Arrange: render Group containing a GroupLabel
        var cut = Render<MenuGroup>(p => p
            .AddChildContent<MenuGroupLabel>(np => np
                .Add(c => c.Id, "my-label")
                .AddChildContent("Actions")));

        // The group's aria-labelledby should point to the label's id.
        var group = cut.Find("[role='group']");
        Assert.Equal("my-label", group.GetAttribute("aria-labelledby"));
    }

    // -- MenuGroupLabel --

    [Fact]
    public void GroupLabel_HasRolePresentation()
    {
        var cut = Render<MenuGroup>(p => p
            .AddChildContent<MenuGroupLabel>(np => np.AddChildContent("Actions")));

        Assert.Equal("presentation", cut.Find("[role='presentation']").GetAttribute("role"));
    }

    // -- MenuCheckboxItem --

    [Fact]
    public void CheckboxItem_HasMenuitemcheckboxRole()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuCheckboxItem>(np => np
                .Add(c => c.Checked, false)
                .AddChildContent("Toggle")));

        Assert.Equal("menuitemcheckbox", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void CheckboxItem_AriaCheckedTrueWhenChecked()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuCheckboxItem>(np => np
                .Add(c => c.Checked, true)
                .AddChildContent("Toggle")));

        Assert.Equal("true", cut.Find("div").GetAttribute("aria-checked"));
        Assert.NotNull(cut.Find("[data-checked]"));
    }

    [Fact]
    public void CheckboxItem_AriaCheckedFalseWhenUnchecked()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuCheckboxItem>(np => np
                .Add(c => c.Checked, false)
                .AddChildContent("Toggle")));

        Assert.Equal("false", cut.Find("div").GetAttribute("aria-checked"));
        Assert.NotNull(cut.Find("[data-unchecked]"));
    }

    [Fact]
    public void CheckboxItem_DisabledHasDataDisabled()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuCheckboxItem>(np => np
                .Add(c => c.Disabled, true)
                .AddChildContent("Toggle")));

        Assert.Equal("-1", cut.Find("div").GetAttribute("tabindex"));
        Assert.NotNull(cut.Find("[data-disabled]"));
    }

    // -- MenuCheckboxItemIndicator --

    [Fact]
    public void CheckboxItemIndicator_RendersWhenChecked()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuCheckboxItem>(np => np
                .Add(c => c.Checked, true)
                .AddChildContent<MenuCheckboxItemIndicator>(ip => ip.AddChildContent("✓"))));

        Assert.Contains("✓", cut.Markup);
        Assert.NotNull(cut.Find("[data-checked]"));
    }

    [Fact]
    public void CheckboxItemIndicator_HiddenWhenUncheckedAndNotKeepMounted()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuCheckboxItem>(np => np
                .Add(c => c.Checked, false)
                .AddChildContent<MenuCheckboxItemIndicator>(ip => ip.AddChildContent("✓"))));

        Assert.DoesNotContain("✓", cut.Markup);
    }

    [Fact]
    public void CheckboxItemIndicator_RendersWhenKeepMounted()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuCheckboxItem>(np => np
                .Add(c => c.Checked, false)
                .AddChildContent<MenuCheckboxItemIndicator>(ip => ip
                    .Add(c => c.KeepMounted, true)
                    .AddChildContent("✓"))));

        Assert.Contains("✓", cut.Markup);
        Assert.NotNull(cut.Find("[data-unchecked]"));
    }

    [Fact]
    public void CheckboxItemIndicator_HasAriaHidden()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuCheckboxItem>(np => np
                .Add(c => c.Checked, true)
                .AddChildContent<MenuCheckboxItemIndicator>(ip => ip.AddChildContent("✓"))));

        Assert.Equal("true", cut.Find("span").GetAttribute("aria-hidden"));
    }

    // -- MenuRadioGroup --

    [Fact]
    public void RadioGroup_HasGroupRole()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuRadioGroup>(np => np.Add(c => c.Value, "opt1")));

        Assert.Equal("group", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void RadioGroup_DisabledHasAriaDisabled()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuRadioGroup>(np => np
                .Add(c => c.Value, "opt1")
                .Add(c => c.Disabled, true)));

        Assert.Equal("true", cut.Find("div").GetAttribute("aria-disabled"));
    }

    // -- MenuRadioItem --
    // Radio items must be nested inside a MenuRadioGroup which cascades the selection context.

    [Fact]
    public void RadioItem_HasMenuitemradioRole()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuRadioGroup>(gp => gp
                .Add(c => c.Value, "opt1")
                .AddChildContent<MenuRadioItem>(np => np
                    .Add(c => c.Value, "opt1")
                    .AddChildContent("Option 1"))));

        Assert.Equal("menuitemradio", cut.Find("[role='menuitemradio']").GetAttribute("role"));
    }

    [Fact]
    public void RadioItem_AriaCheckedTrueWhenMatchesGroupValue()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuRadioGroup>(gp => gp
                .Add(c => c.Value, "opt1")
                .AddChildContent<MenuRadioItem>(np => np
                    .Add(c => c.Value, "opt1")
                    .AddChildContent("Option 1"))));

        Assert.Equal("true", cut.Find("[role='menuitemradio']").GetAttribute("aria-checked"));
        Assert.NotNull(cut.Find("[data-checked]"));
    }

    [Fact]
    public void RadioItem_AriaCheckedFalseWhenDoesNotMatchGroupValue()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuRadioGroup>(gp => gp
                .Add(c => c.Value, "opt1")
                .AddChildContent<MenuRadioItem>(np => np
                    .Add(c => c.Value, "opt2")
                    .AddChildContent("Option 2"))));

        Assert.Equal("false", cut.Find("[role='menuitemradio']").GetAttribute("aria-checked"));
        Assert.NotNull(cut.Find("[data-unchecked]"));
    }

    [Fact]
    public void RadioItem_DisabledHasDataDisabledAndNegativeTabindex()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuRadioGroup>(gp => gp
                .Add(c => c.Value, "opt1")
                .AddChildContent<MenuRadioItem>(np => np
                    .Add(c => c.Value, "opt2")
                    .Add(c => c.Disabled, true)
                    .AddChildContent("Option 2"))));

        Assert.NotNull(cut.Find("[data-disabled]"));
        Assert.Equal("-1", cut.Find("[role='menuitemradio']").GetAttribute("tabindex"));
    }

    [Fact]
    public void RadioItem_GroupDisabledPropagatestoItems()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuRadioGroup>(gp => gp
                .Add(c => c.Value, "opt1")
                .Add(c => c.Disabled, true)
                .AddChildContent<MenuRadioItem>(np => np
                    .Add(c => c.Value, "opt1")
                    .AddChildContent("Option 1"))));

        // Group's aria-disabled is on the group element; item inherits via IsDisabled.
        Assert.NotNull(cut.Find("[role='group'][aria-disabled='true']"));
        Assert.Equal("-1", cut.Find("[role='menuitemradio']").GetAttribute("tabindex"));
        Assert.NotNull(cut.Find("[role='menuitemradio'][data-disabled]"));
    }

    // -- MenuRadioItemIndicator --

    [Fact]
    public void RadioItemIndicator_RendersWhenChecked()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuRadioGroup>(gp => gp
                .Add(c => c.Value, "opt1")
                .AddChildContent<MenuRadioItem>(np => np
                    .Add(c => c.Value, "opt1")
                    .AddChildContent<MenuRadioItemIndicator>(ip => ip.AddChildContent("●")))));

        Assert.Contains("●", cut.Markup);
        Assert.NotNull(cut.Find("[data-checked]"));
    }

    [Fact]
    public void RadioItemIndicator_HiddenWhenNotChecked()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuRadioGroup>(gp => gp
                .Add(c => c.Value, "opt1")
                .AddChildContent<MenuRadioItem>(np => np
                    .Add(c => c.Value, "opt2")
                    .AddChildContent<MenuRadioItemIndicator>(ip => ip.AddChildContent("●")))));

        Assert.DoesNotContain("●", cut.Markup);
    }

    [Fact]
    public void RadioItemIndicator_HasAriaHidden()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuRadioGroup>(gp => gp
                .Add(c => c.Value, "opt1")
                .AddChildContent<MenuRadioItem>(np => np
                    .Add(c => c.Value, "opt1")
                    .AddChildContent<MenuRadioItemIndicator>(ip => ip.AddChildContent("●")))));

        Assert.Equal("true", cut.Find("span").GetAttribute("aria-hidden"));
    }

    // -- MenuTrigger disabled --

    [Fact]
    public void Trigger_DisabledRendersDisabledButton()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuTrigger>(np => np
                .Add(c => c.Disabled, true)
                .AddChildContent("Open")));

        // disabled attribute must be present; aria-expanded reflects closed state.
        var button = cut.Find("button");
        Assert.NotNull(button.GetAttribute("disabled"));
        Assert.Equal("false", button.GetAttribute("aria-expanded"));
    }

    // -- MenuSubmenuTrigger data-highlighted --

    [Fact]
    public void SubmenuTrigger_HighlightedHasDataHighlighted()
    {
        // Provide a context where the highlighted item id matches the trigger's own id.
        var ctx = CreateContext();
        ctx.HighlightedItemId = "my-submenu-trigger";

        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuSubmenuTrigger>(np => np
                .Add(c => c.Id, "my-submenu-trigger")
                .AddChildContent("Sub")));

        Assert.NotNull(cut.Find("[data-highlighted]"));
    }

    [Fact]
    public void SubmenuTrigger_NotHighlightedHasNoDataHighlighted()
    {
        var ctx = CreateContext();

        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuSubmenuTrigger>(np => np
                .Add(c => c.Id, "my-submenu-trigger")
                .AddChildContent("Sub")));

        Assert.Empty(cut.FindAll("[data-highlighted]"));
    }

    // -- MenuCheckboxItemIndicator: no data-highlighted --

    [Fact]
    public void CheckboxItemIndicator_DoesNotEmitDataHighlighted()
    {
        // The indicator emits data-checked/data-unchecked/data-disabled, not data-highlighted.
        var ctx = CreateContext();
        ctx.HighlightedItemId = "highlight-me";

        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuCheckboxItem>(np => np
                .Add(c => c.Id, "highlight-me")
                .Add(c => c.Checked, true)
                .AddChildContent<MenuCheckboxItemIndicator>(ip => ip.AddChildContent("✓"))));

        // data-highlighted must not appear on the indicator span.
        Assert.Empty(cut.FindAll("span[data-highlighted]"));
    }

    // -- MenuRadioItemIndicator: no data-highlighted --

    [Fact]
    public void RadioItemIndicator_DoesNotEmitDataHighlighted()
    {
        var ctx = CreateContext();
        ctx.HighlightedItemId = "highlight-me";

        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuRadioGroup>(gp => gp
                .Add(c => c.Value, "opt1")
                .AddChildContent<MenuRadioItem>(np => np
                    .Add(c => c.Id, "highlight-me")
                    .Add(c => c.Value, "opt1")
                    .AddChildContent<MenuRadioItemIndicator>(ip => ip.AddChildContent("●")))));

        // data-highlighted must not appear on the indicator span.
        Assert.Empty(cut.FindAll("span[data-highlighted]"));
    }

    // -- MenuLinkItem --

    [Fact]
    public void LinkItem_RendersAnchorWithMenuitemRole()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuLinkItem>(np => np.AddChildContent("About")));

        var anchor = cut.Find("a");
        Assert.Equal("A", anchor.TagName);
        Assert.Equal("menuitem", anchor.GetAttribute("role"));
    }

    [Fact]
    public void LinkItem_HighlightedHasDataHighlighted()
    {
        var ctx = CreateContext();
        ctx.HighlightedItemId = "link-about";

        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuLinkItem>(np => np
                .Add(c => c.Id, "link-about")
                .AddChildContent("About")));

        Assert.NotNull(cut.Find("[data-highlighted]"));
    }

    // -- MenuSeparator orientation --

    [Fact]
    public void Separator_DefaultOrientationIsHorizontal()
    {
        var cut = Render<MenuSeparator>();
        Assert.Equal("horizontal", cut.Find("div").GetAttribute("aria-orientation"));
    }

    [Fact]
    public void Separator_VerticalOrientationHasAriaOrientationVertical()
    {
        var cut = Render<MenuSeparator>(p => p
            .Add(c => c.Orientation, BlazeUI.Headless.Core.Orientation.Vertical));

        Assert.Equal("vertical", cut.Find("div").GetAttribute("aria-orientation"));
    }

    // -- MenuPopup --

    [Fact]
    public void Popup_HasMenuRoleWhenMounted()
    {
        // The popup is only mounted after first open.
        var ctx = CreateContext(open: true);

        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuPopup>(np => np.AddChildContent("Content")));

        Assert.Equal("menu", cut.Find("[role='menu']").GetAttribute("role"));
    }

    [Fact]
    public void Popup_HasNegativeTabindex()
    {
        var ctx = CreateContext(open: true);

        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuPopup>(np => np.AddChildContent("Content")));

        Assert.Equal("-1", cut.Find("[role='menu']").GetAttribute("tabindex"));
    }

    [Fact]
    public void Popup_DoesNotRenderDataOpenOrClosed()
    {
        // data-open/data-closed are owned by JS (set after floating-ui positioning)
        // to avoid Blazor triggering CSS animations before the popup is positioned.
        var ctx = CreateContext(open: true);

        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuPopup>());

        Assert.Empty(cut.FindAll("[data-open]"));
        Assert.Empty(cut.FindAll("[data-closed]"));
    }

    [Fact]
    public void Popup_IdMatchesContextPopupId()
    {
        var ctx = CreateContext(open: true);

        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuPopup>());

        Assert.Equal("test-popup", cut.Find("[role='menu']").GetAttribute("id"));
    }

    // -- MenuSubmenuTrigger ARIA --

    [Fact]
    public void SubmenuTrigger_HasAriaHaspopupMenu()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuSubmenuTrigger>(np => np.AddChildContent("Sub")));

        Assert.Equal("menu", cut.Find("div").GetAttribute("aria-haspopup"));
    }

    [Fact]
    public void SubmenuTrigger_AriaExpandedFalseWhenClosed()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuSubmenuTrigger>(np => np.AddChildContent("Sub")));

        Assert.Equal("false", cut.Find("div").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void SubmenuTrigger_AriaExpandedTrueWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuSubmenuTrigger>(np => np.AddChildContent("Sub")));

        Assert.Equal("true", cut.Find("div").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void SubmenuTrigger_HasMenuitemRole()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuSubmenuTrigger>(np => np.AddChildContent("Sub")));

        Assert.Equal("menuitem", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void SubmenuTrigger_DisabledHasDataDisabled()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuSubmenuTrigger>(np => np
                .Add(c => c.Disabled, true)
                .AddChildContent("Sub")));

        Assert.NotNull(cut.Find("[data-disabled]"));
        Assert.Equal("-1", cut.Find("div").GetAttribute("tabindex"));
    }

    // -- MenuCheckboxItem click interaction --

    [Fact]
    public void CheckboxItem_ClickInvokesCheckedChanged()
    {
        var ctx = CreateContext();
        var checkedChangedValue = default(bool?);

        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuCheckboxItem>(np => np
                .Add(c => c.Checked, false)
                .Add(c => c.CheckedChanged, EventCallback.Factory.Create<bool>(this, v => checkedChangedValue = v))
                .AddChildContent("Toggle")));

        // Act: click the checkbox item div.
        cut.Find("div").Click();

        // Assert: the callback was invoked with the toggled value.
        Assert.Equal(true, checkedChangedValue);
    }

    [Fact]
    public void CheckboxItem_DisabledDoesNotInvokeCheckedChanged()
    {
        var ctx = CreateContext();
        var callbackInvoked = false;

        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuCheckboxItem>(np => np
                .Add(c => c.Checked, false)
                .Add(c => c.Disabled, true)
                .Add(c => c.CheckedChanged, EventCallback.Factory.Create<bool>(this, _ => callbackInvoked = true))
                .AddChildContent("Toggle")));

        cut.Find("div").Click();

        Assert.False(callbackInvoked);
    }

    // -- MenuRadioItem click interaction --

    [Fact]
    public void RadioItem_ClickChangesGroupValue()
    {
        var ctx = CreateContext();
        var selectedValue = "opt1";

        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuRadioGroup>(gp => gp
                .Add(c => c.Value, selectedValue)
                .Add(c => c.ValueChanged, EventCallback.Factory.Create<string>(this, v => selectedValue = v))
                .AddChildContent<MenuRadioItem>(np => np
                    .Add(c => c.Value, "opt2")
                    .AddChildContent("Option 2"))));

        // Act: click opt2.
        cut.Find("[role='menuitemradio']").Click();

        Assert.Equal("opt2", selectedValue);
    }

    [Fact]
    public void RadioItem_DisabledDoesNotChangeGroupValue()
    {
        var ctx = CreateContext();
        var selectedValue = "opt1";

        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuRadioGroup>(gp => gp
                .Add(c => c.Value, selectedValue)
                .Add(c => c.ValueChanged, EventCallback.Factory.Create<string>(this, v => selectedValue = v))
                .AddChildContent<MenuRadioItem>(np => np
                    .Add(c => c.Value, "opt2")
                    .Add(c => c.Disabled, true)
                    .AddChildContent("Option 2"))));

        cut.Find("[role='menuitemradio']").Click();

        // Value should not have changed.
        Assert.Equal("opt1", selectedValue);
    }

    // -- MenuViewport --

    [Fact]
    public void Viewport_RendersDiv()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuViewport>(np => np.AddChildContent("content")));

        Assert.Equal("DIV", cut.Find("div").TagName);
    }

    // -- MenuItem click invokes OnClick callback --

    [Fact]
    public void Item_ClickInvokesOnClick()
    {
        var ctx = CreateContext();
        var clicked = false;

        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuItem>(np => np
                .Add(c => c.OnClick, EventCallback.Factory.Create(this, () => clicked = true))
                .AddChildContent("Cut")));

        cut.Find("div").Click();

        Assert.True(clicked);
    }

    [Fact]
    public void Item_DisabledDoesNotInvokeOnClick()
    {
        var ctx = CreateContext();
        var clicked = false;

        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuItem>(np => np
                .Add(c => c.Disabled, true)
                .Add(c => c.OnClick, EventCallback.Factory.Create(this, () => clicked = true))
                .AddChildContent("Cut")));

        cut.Find("div").Click();

        Assert.False(clicked);
    }

    // -- Trigger data-pressed matches data-popup-open --

    [Fact]
    public void Trigger_HasDataPressedWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuTrigger>(np => np.AddChildContent("Open")));

        // Base UI's pressableTriggerOpenStateMapping emits both data-popup-open and data-pressed.
        Assert.NotNull(cut.Find("[data-pressed]"));
    }

    [Fact]
    public void Trigger_NoDataPressedWhenClosed()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<MenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MenuTrigger>(np => np.AddChildContent("Open")));

        Assert.Empty(cut.FindAll("[data-pressed]"));
    }
}
