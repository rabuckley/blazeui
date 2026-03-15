using BlazeUI.Headless.Components.Tooltip;
using BlazeUI.Headless.Core;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Headless.Tests.Components.Tooltip;

public class TooltipTests : BunitContext
{
    public TooltipTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazeUI();
    }

    // Context helpers for testing child components in isolation (without a full Root).

    private TooltipContext CreateContext(
        bool open = false,
        bool disabled = false,
        string triggerId = "test-trigger",
        string popupId = "test-popup",
        string positionerId = "test-positioner",
        string side = "top",
        string align = "center")
        => new()
        {
            Open = open,
            Disabled = disabled,
            TriggerId = triggerId,
            PopupId = popupId,
            PositionerId = positionerId,
            Side = side,
            Align = align,
        };

    // ── TooltipTrigger ───────────────────────────────────────────────────────

    [Fact]
    public void Trigger_RendersButtonByDefault()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipTrigger>(t => t.AddChildContent("Hover me")));

        Assert.NotNull(cut.Find("button"));
    }

    [Fact]
    public void Trigger_HasTriggerIdAsHtmlId()
    {
        var ctx = CreateContext(triggerId: "my-trigger");
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipTrigger>(t => t.AddChildContent("Hover me")));

        Assert.Equal("my-trigger", cut.Find("button").GetAttribute("id"));
    }

    [Fact]
    public void Trigger_HasDataPopupOpenWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipTrigger>(t => t.AddChildContent("Hover me")));

        Assert.NotNull(cut.Find("[data-popup-open]"));
    }

    [Fact]
    public void Trigger_NoDataPopupOpenWhenClosed()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipTrigger>(t => t.AddChildContent("Hover me")));

        Assert.Empty(cut.FindAll("[data-popup-open]"));
    }

    [Fact]
    public void Trigger_HasAriaDescribedByWhenOpen()
    {
        var ctx = CreateContext(open: true, popupId: "my-popup");
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipTrigger>(t => t.AddChildContent("Hover me")));

        Assert.Equal("my-popup", cut.Find("button").GetAttribute("aria-describedby"));
    }

    [Fact]
    public void Trigger_NoAriaDescribedByWhenClosed()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipTrigger>(t => t.AddChildContent("Hover me")));

        Assert.Null(cut.Find("button").GetAttribute("aria-describedby"));
    }

    [Fact]
    public void Trigger_HasDataTriggerDisabledWhenContextDisabled()
    {
        // Root-level Disabled flag flows into context and triggers emit data-trigger-disabled.
        var ctx = CreateContext(disabled: true);
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipTrigger>(t => t.AddChildContent("Hover me")));

        Assert.NotNull(cut.Find("[data-trigger-disabled]"));
    }

    [Fact]
    public void Trigger_NoDataTriggerDisabledByDefault()
    {
        var ctx = CreateContext(disabled: false);
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipTrigger>(t => t.AddChildContent("Hover me")));

        Assert.Empty(cut.FindAll("[data-trigger-disabled]"));
    }

    [Fact]
    public void Trigger_PerTriggerDisabledOverridesRoot()
    {
        // Root is not disabled, but per-trigger Disabled=true should set data-trigger-disabled.
        var ctx = CreateContext(disabled: false);
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipTrigger>(t => t
                .Add(c => c.Disabled, true)
                .AddChildContent("Hover me")));

        Assert.NotNull(cut.Find("[data-trigger-disabled]"));
    }

    // ── TooltipPopup ─────────────────────────────────────────────────────────

    [Fact]
    public void Popup_RoleIsTooltip()
    {
        // The popup must have role="tooltip" to satisfy the ARIA tooltip pattern.
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipPopup>(popup => popup.AddChildContent("Tooltip content")));

        Assert.Equal("tooltip", cut.Find("[role='tooltip']").GetAttribute("role"));
    }

    [Fact]
    public void Popup_HasPopupIdAsHtmlId()
    {
        var ctx = CreateContext(open: true, popupId: "my-popup");
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipPopup>(popup => popup.AddChildContent("Content")));

        Assert.Equal("my-popup", cut.Find("[role='tooltip']").GetAttribute("id"));
    }

    // data-open/data-closed are owned by JS (not rendered by Blazor) to
    // avoid re-render cycles resetting animation state. Verified by E2E tests.

    [Fact]
    public void Popup_HasDataSideFromContext()
    {
        var ctx = CreateContext(open: true, side: "bottom");
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipPopup>(popup => popup.AddChildContent("Content")));

        Assert.Equal("bottom", cut.Find("[role='tooltip']").GetAttribute("data-side"));
    }

    [Fact]
    public void Popup_HasDataAlignFromContext()
    {
        var ctx = CreateContext(open: true, align: "start");
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipPopup>(popup => popup.AddChildContent("Content")));

        Assert.Equal("start", cut.Find("[role='tooltip']").GetAttribute("data-align"));
    }

    [Fact]
    public void Popup_NotRenderedWhenNeverOpened()
    {
        // Popup should not mount until the first time Open transitions to true.
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipPopup>(popup => popup.AddChildContent("Content")));

        Assert.Empty(cut.FindAll("[role='tooltip']"));
    }

    [Fact]
    public void Popup_RendersChildContent()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipPopup>(popup => popup.AddChildContent("My tooltip text")));

        Assert.Contains("My tooltip text", cut.Markup);
    }

    // ── TooltipPositioner ────────────────────────────────────────────────────

    [Fact]
    public void Positioner_RoleIsPresentation()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipPositioner>());

        Assert.Equal("presentation", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void Positioner_HasPositionerIdAsHtmlId()
    {
        var ctx = CreateContext(positionerId: "my-positioner");
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipPositioner>());

        Assert.Equal("my-positioner", cut.Find("div").GetAttribute("id"));
    }

    [Fact]
    public void Positioner_HasDataOpenWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipPositioner>());

        Assert.NotNull(cut.Find("[data-open]"));
    }

    [Fact]
    public void Positioner_HasDataClosedWhenClosed()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipPositioner>());

        Assert.NotNull(cut.Find("[data-closed]"));
    }

    [Fact]
    public void Positioner_HasDataSideDefault()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipPositioner>(pos => pos
                .Add(c => c.Side, Side.Top)));

        Assert.Equal("top", cut.Find("div").GetAttribute("data-side"));
    }

    [Fact]
    public void Positioner_HasDataAlignDefault()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipPositioner>(pos => pos
                .Add(c => c.Align, Align.Center)));

        Assert.Equal("center", cut.Find("div").GetAttribute("data-align"));
    }

    [Fact]
    public void Positioner_PropagatesSideToContext()
    {
        // When TooltipPositioner renders, it should update Context.Side so popup
        // and arrow can pick it up.
        var ctx = CreateContext(open: true);
        Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipPositioner>(pos => pos
                .Add(c => c.Side, Side.Bottom)));

        Assert.Equal("bottom", ctx.Side);
    }

    [Fact]
    public void Positioner_PropagatesAlignToContext()
    {
        var ctx = CreateContext(open: true);
        Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipPositioner>(pos => pos
                .Add(c => c.Align, Align.End)));

        Assert.Equal("end", ctx.Align);
    }

    // ── TooltipArrow ─────────────────────────────────────────────────────────

    [Fact]
    public void Arrow_RendersDiv()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipArrow>());

        Assert.NotNull(cut.Find("div"));
    }

    [Fact]
    public void Arrow_HasAriaHidden()
    {
        // Arrows are decorative; they must be hidden from assistive technology.
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipArrow>());

        Assert.Equal("true", cut.Find("div").GetAttribute("aria-hidden"));
    }

    [Fact]
    public void Arrow_HasDataOpenWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipArrow>());

        Assert.NotNull(cut.Find("[data-open]"));
    }

    [Fact]
    public void Arrow_HasDataClosedWhenClosed()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipArrow>());

        Assert.NotNull(cut.Find("[data-closed]"));
    }

    [Fact]
    public void Arrow_HasDataSideFromContext()
    {
        var ctx = CreateContext(open: true, side: "left");
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipArrow>());

        Assert.Equal("left", cut.Find("div").GetAttribute("data-side"));
    }

    [Fact]
    public void Arrow_HasDataAlignFromContext()
    {
        var ctx = CreateContext(open: true, align: "end");
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipArrow>());

        Assert.Equal("end", cut.Find("div").GetAttribute("data-align"));
    }

    // ── TooltipRoot (integration) ─────────────────────────────────────────────

    [Fact]
    public void Root_RendersChildContent()
    {
        var cut = Render<TooltipRoot>(p => p
            .AddChildContent<TooltipTrigger>(t => t.AddChildContent("Hover me")));

        Assert.NotNull(cut.Find("button"));
        Assert.Contains("Hover me", cut.Markup);
    }

    [Fact]
    public void Root_DefaultOpen_TriggerHasDataPopupOpen()
    {
        // When DefaultOpen=true, the root opens on first render and the trigger
        // should emit data-popup-open to signal its connected tooltip is visible.
        var cut = Render<TooltipRoot>(p => p
            .Add(c => c.DefaultOpen, true)
            .AddChildContent<TooltipTrigger>(t => t.AddChildContent("Hover me")));

        Assert.NotNull(cut.Find("[data-popup-open]"));
    }

    [Fact]
    public void Root_Disabled_ContextHasDisabledTrue()
    {
        // Verify that Disabled=true on Root causes trigger to emit data-trigger-disabled.
        var cut = Render<TooltipRoot>(p => p
            .Add(c => c.Disabled, true)
            .AddChildContent<TooltipTrigger>(t => t.AddChildContent("Hover me")));

        Assert.NotNull(cut.Find("[data-trigger-disabled]"));
    }

    // ── TooltipViewport ───────────────────────────────────────────────────────

    [Fact]
    public void Viewport_RendersDiv()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<TooltipContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<TooltipViewport>(v => v.AddChildContent("viewport content")));

        Assert.NotNull(cut.Find("div"));
        Assert.Contains("viewport content", cut.Markup);
    }
}
