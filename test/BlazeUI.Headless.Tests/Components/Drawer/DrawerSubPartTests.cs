using BlazeUI.Headless.Components.Drawer;
using BlazeUI.Headless.Core;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Headless.Tests.Components.Drawer;

public class DrawerSubPartTests : BunitContext
{
    public DrawerSubPartTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazeUI();
    }

    private DrawerContext CreateContext(bool open = false, SwipeDirection swipeDirection = SwipeDirection.Down) => new()
    {
        Open = open,
        SwipeDirection = swipeDirection,
        PopupId = "test-drawer-popup",
    };

    // -- DrawerContent --

    [Fact]
    public void DrawerContent_HasDataDrawerContentAttribute()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<DrawerContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DrawerContent>(cp => cp.AddChildContent("Content")));

        Assert.NotNull(cut.Find("[data-drawer-content]"));
    }

    [Fact]
    public void DrawerContent_DoesNotHaveDataOpenOrDataClosed()
    {
        // Base UI's DrawerContent only emits data-drawer-content, not data-open/data-closed.
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<DrawerContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DrawerContent>(cp => cp.AddChildContent("Content")));

        var el = cut.Find("[data-drawer-content]");
        Assert.Null(el.GetAttribute("data-open"));
        Assert.Null(el.GetAttribute("data-closed"));
    }

    [Fact]
    public void DrawerContent_RendersChildContent()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<DrawerContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DrawerContent>(cp => cp.AddChildContent("My drawer content")));

        Assert.Contains("My drawer content", cut.Markup);
    }

    // -- DrawerProvider / DrawerProviderContext --

    [Fact]
    public void DrawerProviderContext_InactiveByDefault()
    {
        var ctx = new DrawerProviderContext();
        Assert.False(ctx.Active);
    }

    [Fact]
    public void DrawerProviderContext_ActiveWhenDrawerOpen()
    {
        var ctx = new DrawerProviderContext();
        ctx.SetDrawerOpen("drawer-1", true);
        Assert.True(ctx.Active);
    }

    [Fact]
    public void DrawerProviderContext_InactiveWhenAllDrawersClosed()
    {
        var ctx = new DrawerProviderContext();
        ctx.SetDrawerOpen("drawer-1", true);
        ctx.SetDrawerOpen("drawer-1", false);
        Assert.False(ctx.Active);
    }

    [Fact]
    public void DrawerProviderContext_ActiveWhenAnyDrawerOpen()
    {
        var ctx = new DrawerProviderContext();
        ctx.SetDrawerOpen("drawer-1", true);
        ctx.SetDrawerOpen("drawer-2", false);
        Assert.True(ctx.Active);
    }

    [Fact]
    public void DrawerProviderContext_RemoveDrawer()
    {
        var ctx = new DrawerProviderContext();
        ctx.SetDrawerOpen("drawer-1", true);
        ctx.RemoveDrawer("drawer-1");
        Assert.False(ctx.Active);
    }

    [Fact]
    public void DrawerProvider_RendersChildren()
    {
        var cut = Render<DrawerProvider>(p => p
            .AddChildContent((RenderFragment)(b => b.AddContent(0, "Provider content"))));

        Assert.Contains("Provider content", cut.Markup);
    }

    // -- DrawerIndent --

    [Fact]
    public void DrawerIndent_HasDataInactiveWhenNoProvider()
    {
        var cut = Render<DrawerIndent>(p => p.AddChildContent("Main content"));

        Assert.NotNull(cut.Find("[data-inactive]"));
        Assert.Empty(cut.FindAll("[data-active]"));
    }

    [Fact]
    public void DrawerIndent_HasDataActiveWhenProviderActive()
    {
        var providerCtx = new DrawerProviderContext();
        providerCtx.SetDrawerOpen("test", true);

        var cut = Render<CascadingValue<DrawerProviderContext>>(p => p
            .Add(c => c.Value, providerCtx)
            .AddChildContent<DrawerIndent>(dp => dp.AddChildContent("Main content")));

        Assert.NotNull(cut.Find("[data-active]"));
        Assert.Empty(cut.FindAll("[data-inactive]"));
    }

    // -- DrawerIndentBackground --

    [Fact]
    public void DrawerIndentBackground_HasDataInactiveByDefault()
    {
        var cut = Render<DrawerIndentBackground>(p => p.AddChildContent("BG"));

        Assert.NotNull(cut.Find("[data-inactive]"));
    }

    [Fact]
    public void DrawerIndentBackground_HasDataActiveWhenProviderActive()
    {
        var providerCtx = new DrawerProviderContext();
        providerCtx.SetDrawerOpen("test", true);

        var cut = Render<CascadingValue<DrawerProviderContext>>(p => p
            .Add(c => c.Value, providerCtx)
            .AddChildContent<DrawerIndentBackground>(dp => dp.AddChildContent("BG")));

        Assert.NotNull(cut.Find("[data-active]"));
    }

    // -- DrawerSwipeArea --

    [Fact]
    public void DrawerSwipeArea_HasPresentationRole()
    {
        var ctx = CreateContext(open: false, SwipeDirection.Right);
        var cut = Render<CascadingValue<DrawerContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DrawerSwipeArea>());

        Assert.Equal("presentation", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void DrawerSwipeArea_HasAriaHidden()
    {
        var ctx = CreateContext(open: false, SwipeDirection.Down);
        var cut = Render<CascadingValue<DrawerContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DrawerSwipeArea>());

        Assert.Equal("true", cut.Find("div").GetAttribute("aria-hidden"));
    }

    [Fact]
    public void DrawerSwipeArea_HasSwipeDirectionDataAttribute()
    {
        // Root swipeDirection is Right (dismiss direction), so SwipeArea open direction is Left.
        var ctx = CreateContext(open: false, SwipeDirection.Right);
        var cut = Render<CascadingValue<DrawerContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DrawerSwipeArea>());

        // SwipeArea defaults to the opposite of the root's dismiss direction.
        Assert.Equal("left", cut.Find("div").GetAttribute("data-swipe-direction"));
    }

    [Fact]
    public void DrawerSwipeArea_DisabledSetsDataAttribute()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<DrawerContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DrawerSwipeArea>(sp => sp.Add(c => c.Disabled, true)));

        Assert.NotNull(cut.Find("[data-disabled]"));
    }

    [Fact]
    public void DrawerSwipeArea_CustomSwipeDirectionOverridesDefault()
    {
        // When SwipeDirection is set explicitly, it overrides the opposite-of-root default.
        var ctx = CreateContext(open: false, SwipeDirection.Down);
        var cut = Render<CascadingValue<DrawerContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DrawerSwipeArea>(sp => sp.Add(c => c.SwipeDirection, SwipeDirection.Right)));

        Assert.Equal("right", cut.Find("div").GetAttribute("data-swipe-direction"));
    }

    // -- DrawerViewport --

    [Fact]
    public void DrawerViewport_HasDataOpenWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<DrawerContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DrawerViewport>(vp => vp.AddChildContent("Viewport content")));

        Assert.NotNull(cut.Find("[data-open]"));
        Assert.Empty(cut.FindAll("[data-closed]"));
    }

    [Fact]
    public void DrawerViewport_HasDataClosedWhenClosed()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<DrawerContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DrawerViewport>(vp => vp.AddChildContent("Viewport content")));

        Assert.NotNull(cut.Find("[data-closed]"));
        Assert.Empty(cut.FindAll("[data-open]"));
    }

    [Fact]
    public void DrawerViewport_RendersChildContent()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<DrawerContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DrawerViewport>(vp => vp.AddChildContent("Viewport content")));

        Assert.Contains("Viewport content", cut.Markup);
    }

    // -- DrawerPopup data-swipe-direction --

    [Fact]
    public void DrawerPopup_HasDataSwipeDirectionAttribute()
    {
        // DrawerPopup emits data-swipe-direction matching the root's SwipeDirection.
        // data-open/data-closed are owned by JS, so find the popup via role.
        var ctx = CreateContext(open: true, SwipeDirection.Left);
        var cut = Render<CascadingValue<DrawerContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DrawerPopup>(pp => pp.AddChildContent("Popup")));

        var popup = cut.Find("[role=dialog]");
        Assert.Equal("left", popup.GetAttribute("data-swipe-direction"));
    }

    [Fact]
    public void DrawerPopup_EmitsDataSideAttribute()
    {
        // data-side is the anchor edge (opposite of swipe direction), used by
        // styled templates for direction-specific classes (e.g. drag handle visibility).
        // data-open/data-closed are owned by JS, so find the popup via role.
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<DrawerContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DrawerPopup>(pp => pp.AddChildContent("Popup")));

        var popup = cut.Find("[role=dialog]");
        Assert.Equal("bottom", popup.GetAttribute("data-side"));
    }
}
