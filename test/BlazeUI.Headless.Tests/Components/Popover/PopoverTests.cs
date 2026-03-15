using BlazeUI.Headless.Components.Popover;
using BlazeUI.Headless.Core;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Headless.Tests.Components.Popover;

public class PopoverTests : BunitContext
{
    public PopoverTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazeUI();
    }

    private PopoverContext CreateContext(bool open = false) => new()
    {
        Open = open,
        TriggerId = "test-trigger",
        PopupId = "test-popup",
    };

    // -- Trigger --

    [Fact]
    public void Trigger_RendersButton()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverTrigger>(t => t.AddChildContent("Open")));

        Assert.Equal("BUTTON", cut.Find("button").TagName);
    }

    [Fact]
    public void Trigger_AriaExpandedFalseWhenClosed()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverTrigger>(t => t.AddChildContent("Open")));

        Assert.Equal("false", cut.Find("button").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void Trigger_AriaExpandedTrueWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverTrigger>(t => t.AddChildContent("Open")));

        Assert.Equal("true", cut.Find("button").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void Trigger_AriaControlsPointsToPopupId()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverTrigger>(t => t.AddChildContent("Open")));

        Assert.Equal("test-popup", cut.Find("button").GetAttribute("aria-controls"));
    }

    [Fact]
    public void Trigger_NoAriaHaspopupAttribute()
    {
        // Base UI does not set aria-haspopup on the popover trigger.
        var ctx = CreateContext();
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverTrigger>(t => t.AddChildContent("Open")));

        Assert.Null(cut.Find("button").GetAttribute("aria-haspopup"));
    }

    [Fact]
    public void Trigger_DataPopupOpenPresentWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverTrigger>(t => t.AddChildContent("Open")));

        Assert.NotNull(cut.Find("[data-popup-open]"));
    }

    [Fact]
    public void Trigger_DataPopupOpenAbsentWhenClosed()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverTrigger>(t => t.AddChildContent("Open")));

        Assert.Empty(cut.FindAll("[data-popup-open]"));
    }

    [Fact]
    public void Trigger_UsesContextTriggerId()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverTrigger>(t => t.AddChildContent("Open")));

        Assert.Equal("test-trigger", cut.Find("button").GetAttribute("id"));
    }

    // -- Popup --

    [Fact]
    public void Popup_NotRenderedWhenNeverOpened()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverPopup>(pp => pp.AddChildContent("Content")));

        Assert.Empty(cut.FindAll("div"));
    }

    [Fact]
    public void Popup_RendersWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverPopup>(pp => pp.AddChildContent("Content")));

        Assert.Contains("Content", cut.Markup);
    }

    [Fact]
    public void Popup_HasDialogRole()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverPopup>(pp => pp.AddChildContent("Content")));

        Assert.Equal("dialog", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void Popup_UsesContextPopupId()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverPopup>(pp => pp.AddChildContent("Content")));

        Assert.Equal("test-popup", cut.Find("div").GetAttribute("id"));
    }

    [Fact]
    public void Popup_DoesNotRenderDataOpenOrClosed()
    {
        // data-open/data-closed are owned by JS (set after floating-ui positioning)
        // to avoid Blazor triggering CSS animations before the popup is positioned.
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverPopup>(pp => pp.AddChildContent("Content")));

        Assert.Empty(cut.FindAll("[data-open]"));
        Assert.Empty(cut.FindAll("[data-closed]"));
    }

    [Fact]
    public void Popup_AriaLabelledBySetWhenTitlePresent()
    {
        var ctx = CreateContext(open: true);
        ctx.TitleId = "my-title";
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverPopup>(pp => pp.AddChildContent("Content")));

        Assert.Equal("my-title", cut.Find("div").GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void Popup_AriaDescribedBySetWhenDescriptionPresent()
    {
        var ctx = CreateContext(open: true);
        ctx.DescriptionId = "my-desc";
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverPopup>(pp => pp.AddChildContent("Content")));

        Assert.Equal("my-desc", cut.Find("div").GetAttribute("aria-describedby"));
    }

    // -- Backdrop --

    [Fact]
    public void Backdrop_HasDataOpenWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverBackdrop>());

        Assert.NotNull(cut.Find("[data-open]"));
        Assert.Empty(cut.FindAll("[data-closed]"));
    }

    [Fact]
    public void Backdrop_HasDataClosedWhenClosed()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverBackdrop>());

        Assert.NotNull(cut.Find("[data-closed]"));
        Assert.Empty(cut.FindAll("[data-open]"));
    }

    [Fact]
    public void Backdrop_HasPresentationRole()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverBackdrop>());

        Assert.Equal("presentation", cut.Find("div").GetAttribute("role"));
    }

    // -- Arrow --

    [Fact]
    public void Arrow_RendersDiv()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverArrow>());

        Assert.Equal("DIV", cut.Find("div").TagName);
    }

    [Fact]
    public void Arrow_HasAriaHidden()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverArrow>());

        Assert.Equal("true", cut.Find("div").GetAttribute("aria-hidden"));
    }

    [Fact]
    public void Arrow_HasDataOpenWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverArrow>());

        Assert.NotNull(cut.Find("[data-open]"));
    }

    [Fact]
    public void Arrow_HasDataClosedWhenClosed()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverArrow>());

        Assert.NotNull(cut.Find("[data-closed]"));
    }

    // -- Close --

    [Fact]
    public void Close_RendersButton()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverClose>(c => c.AddChildContent("Close")));

        Assert.Equal("BUTTON", cut.Find("button").TagName);
    }

    [Fact]
    public void Close_ClickCallsSetOpen()
    {
        var setOpenCalled = false;
        bool? setOpenValue = null;
        var ctx = CreateContext(open: true);
        ctx.SetOpen = v => { setOpenCalled = true; setOpenValue = v; return Task.CompletedTask; };

        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverClose>(c => c.AddChildContent("Close")));

        cut.Find("button").Click();

        Assert.True(setOpenCalled);
        Assert.False(setOpenValue);
    }

    // -- Title --

    [Fact]
    public void Title_RendersH2()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverTitle>(t => t.AddChildContent("My Popover")));

        Assert.Equal("H2", cut.Find("h2").TagName);
        Assert.Contains("My Popover", cut.Markup);
    }

    [Fact]
    public void Title_RegistersIdWithContext()
    {
        var ctx = CreateContext();
        Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverTitle>(t => t.AddChildContent("Title")));

        Assert.NotNull(ctx.TitleId);
        Assert.NotEmpty(ctx.TitleId);
    }

    // -- Description --

    [Fact]
    public void Description_RendersParagraph()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverDescription>(d => d.AddChildContent("Details here.")));

        Assert.Equal("P", cut.Find("p").TagName);
        Assert.Contains("Details here.", cut.Markup);
    }

    [Fact]
    public void Description_RegistersIdWithContext()
    {
        var ctx = CreateContext();
        Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverDescription>(d => d.AddChildContent("Details")));

        Assert.NotNull(ctx.DescriptionId);
        Assert.NotEmpty(ctx.DescriptionId);
    }

    // -- Positioner --

    [Fact]
    public void Positioner_HasDataSideAndDataAlign()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverPositioner>());

        var div = cut.Find("div");
        // Defaults are bottom/center.
        Assert.Equal("bottom", div.GetAttribute("data-side"));
        Assert.Equal("center", div.GetAttribute("data-align"));
    }

    [Fact]
    public void Positioner_HasDataOpenWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverPositioner>());

        Assert.NotNull(cut.Find("[data-open]"));
        Assert.Empty(cut.FindAll("[data-closed]"));
    }

    [Fact]
    public void Positioner_HasDataClosedWhenClosed()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverPositioner>());

        Assert.NotNull(cut.Find("[data-closed]"));
        Assert.Empty(cut.FindAll("[data-open]"));
    }

    // -- Viewport --

    [Fact]
    public void Viewport_RendersDiv()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<PopoverContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<PopoverViewport>(v => v.AddChildContent("Content")));

        Assert.Equal("DIV", cut.Find("div").TagName);
    }
}
