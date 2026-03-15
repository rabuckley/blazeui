using BlazeUI.Headless.Components.NavigationMenu;
using BlazeUI.Headless.Core;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Headless.Tests.Components.NavigationMenu;

public class NavigationMenuSubPartTests : BunitContext
{
    public NavigationMenuSubPartTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazeUI();
    }

    private NavigationMenuContext CreateContext(string? activeValue = null, string? previousValue = null,
        string? activationDirection = null) => new()
    {
        ActiveValue = activeValue,
        PreviousValue = previousValue,
        ActivationDirection = activationDirection,
        Orientation = Orientation.Horizontal,
        RootId = "test-navmenu",
        ListId = "test-navmenu-list",
        ViewportId = "test-navmenu-viewport",
    };

    // -- NavigationMenuIcon --

    [Fact]
    public void Icon_RendersSpanByDefault()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<NavigationMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<NavigationMenuIcon>(ip => ip
                .Add(c => c.Value, "products")
                .AddChildContent("▼")));

        Assert.Equal("SPAN", cut.Find("span").TagName);
    }

    [Fact]
    public void Icon_HasAriaHidden()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<NavigationMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<NavigationMenuIcon>(ip => ip
                .Add(c => c.Value, "products")
                .AddChildContent("▼")));

        Assert.Equal("true", cut.Find("span").GetAttribute("aria-hidden"));
    }

    [Fact]
    public void Icon_HasNoDataPopupOpenWhenInactive()
    {
        // Base UI uses triggerOpenStateMapping (data-popup-open) rather than
        // popupStateMapping (data-open / data-closed) for the icon. Absent = closed.
        var ctx = CreateContext(activeValue: null);
        var cut = Render<CascadingValue<NavigationMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<NavigationMenuIcon>(ip => ip
                .Add(c => c.Value, "products")
                .AddChildContent("▼")));

        Assert.False(cut.Find("span").HasAttribute("data-popup-open"));
        Assert.Empty(cut.FindAll("[data-open]"));
        Assert.Empty(cut.FindAll("[data-closed]"));
    }

    [Fact]
    public void Icon_HasDataPopupOpenWhenActive()
    {
        // Base UI uses data-popup-open (not data-open) to indicate the icon's active state.
        var ctx = CreateContext(activeValue: "products");
        var cut = Render<CascadingValue<NavigationMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<NavigationMenuIcon>(ip => ip
                .Add(c => c.Value, "products")
                .AddChildContent("▼")));

        Assert.True(cut.Find("span").HasAttribute("data-popup-open"));
        Assert.Empty(cut.FindAll("[data-closed]"));
    }

    [Fact]
    public void Icon_NoDataPopupOpenWhenDifferentItemActive()
    {
        // When a different item is active the icon has no data-popup-open (absent = closed);
        // Base UI does not emit data-closed on the icon.
        var ctx = CreateContext(activeValue: "docs");
        var cut = Render<CascadingValue<NavigationMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<NavigationMenuIcon>(ip => ip
                .Add(c => c.Value, "products")
                .AddChildContent("▼")));

        Assert.False(cut.Find("span").HasAttribute("data-popup-open"));
        Assert.Empty(cut.FindAll("[data-closed]"));
    }

    // -- NavigationMenuContent --
    // Content registers a RenderFragment with the context; Viewport renders it.
    // These tests include both Content and Viewport in the tree.

    [Fact]
    public void Content_RendersWhenActive()
    {
        var ctx = CreateContext(activeValue: "products");
        ctx.RegisterItem("products", "trigger-products", "content-products");

        var cut = RenderContentWithViewport(ctx, "products", "Content here");

        // Viewport div + content div
        var divs = cut.FindAll("div");
        Assert.Equal(2, divs.Count);
    }

    [Fact]
    public void Content_DoesNotRenderWhenInactiveAndNotPrevious()
    {
        var ctx = CreateContext(activeValue: null, previousValue: null);
        ctx.RegisterItem("products", "trigger-products", "content-products");

        var cut = RenderContentWithViewport(ctx, "products", "Content here");

        // Only the viewport div, no content div
        Assert.Single(cut.FindAll("div"));
    }

    [Fact]
    public void Content_HasDataOpenWhenActive()
    {
        var ctx = CreateContext(activeValue: "products");
        ctx.RegisterItem("products", "trigger-products", "content-products");

        var cut = RenderContentWithViewport(ctx, "products", "Content here");

        var content = cut.Find("#content-products");
        Assert.True(content.HasAttribute("data-open"));
        Assert.False(content.HasAttribute("data-closed"));
    }

    [Fact]
    public void Content_HasDataClosedWhenPreviousButNotActive()
    {
        // Previous value: the content stays mounted during exit animation.
        var ctx = CreateContext(activeValue: "docs", previousValue: "products");
        ctx.RegisterItem("products", "trigger-products", "content-products");

        var cut = RenderContentWithViewport(ctx, "products", "Content here");

        var content = cut.Find("#content-products");
        Assert.True(content.HasAttribute("data-closed"));
        Assert.False(content.HasAttribute("data-open"));
    }

    [Fact]
    public void Content_HasDataActivationDirectionWhenOpen()
    {
        var ctx = CreateContext(activeValue: "products", activationDirection: "right");
        ctx.RegisterItem("products", "trigger-products", "content-products");

        var cut = RenderContentWithViewport(ctx, "products", "Content here");

        Assert.Equal("right", cut.Find("#content-products").GetAttribute("data-activation-direction"));
    }

    [Fact]
    public void Content_HasDataActivationDirectionWhenClosing()
    {
        var ctx = CreateContext(activeValue: "docs", previousValue: "products", activationDirection: "right");
        ctx.RegisterItem("products", "trigger-products", "content-products");

        var cut = RenderContentWithViewport(ctx, "products", "Content here");

        // data-activation-direction is set on closing content so CSS can
        // apply directional slide-out transitions.
        Assert.Equal("right", cut.Find("#content-products").GetAttribute("data-activation-direction"));
    }

    // -- NavigationMenuPopup --

    [Fact]
    public void Popup_RendersNavByDefault()
    {
        var ctx = CreateContext(activeValue: "products");
        var cut = Render<CascadingValue<NavigationMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<NavigationMenuPopup>(pp => pp.AddChildContent("Content")));

        Assert.Equal("NAV", cut.Find("nav").TagName);
    }

    [Fact]
    public void Popup_HasTabIndexMinusOne()
    {
        var ctx = CreateContext(activeValue: "products");
        var cut = Render<CascadingValue<NavigationMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<NavigationMenuPopup>(pp => pp.AddChildContent("Content")));

        Assert.Equal("-1", cut.Find("nav").GetAttribute("tabindex"));
    }

    [Fact]
    public void Popup_HasDataOpenWhenActive()
    {
        var ctx = CreateContext(activeValue: "products");
        var cut = Render<CascadingValue<NavigationMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<NavigationMenuPopup>(pp => pp.AddChildContent("Content")));

        Assert.NotNull(cut.Find("[data-open]"));
    }

    [Fact]
    public void Popup_HasDataClosedWhenInactive()
    {
        var ctx = CreateContext(activeValue: null);
        var cut = Render<CascadingValue<NavigationMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<NavigationMenuPopup>(pp => pp.AddChildContent("Content")));

        Assert.NotNull(cut.Find("[data-closed]"));
    }

    // -- NavigationMenuPositioner --

    [Fact]
    public void Positioner_HasPresentationRole()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<NavigationMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<NavigationMenuPositioner>(pp => pp.AddChildContent("Content")));

        Assert.Equal("presentation", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void Positioner_HasSideDataAttribute()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<NavigationMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<NavigationMenuPositioner>(pp => pp
                .Add(c => c.Side, Side.Bottom)
                .AddChildContent("Content")));

        Assert.Equal("bottom", cut.Find("div").GetAttribute("data-side"));
    }

    [Fact]
    public void Positioner_HasAlignDataAttribute()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<NavigationMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<NavigationMenuPositioner>(pp => pp
                .Add(c => c.Align, Align.Center)
                .AddChildContent("Content")));

        Assert.Equal("center", cut.Find("div").GetAttribute("data-align"));
    }

    // -- NavigationMenuArrow --

    [Fact]
    public void Arrow_HasAriaHidden()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<NavigationMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<NavigationMenuArrow>());

        Assert.Equal("true", cut.Find("div").GetAttribute("aria-hidden"));
    }

    [Fact]
    public void Arrow_HasDataOpenWhenActive()
    {
        var ctx = CreateContext(activeValue: "products");
        var cut = Render<CascadingValue<NavigationMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<NavigationMenuArrow>());

        Assert.NotNull(cut.Find("[data-open]"));
    }

    [Fact]
    public void Arrow_HasDataClosedWhenInactive()
    {
        var ctx = CreateContext(activeValue: null);
        var cut = Render<CascadingValue<NavigationMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<NavigationMenuArrow>());

        Assert.NotNull(cut.Find("[data-closed]"));
    }

    // -- NavigationMenuBackdrop --

    [Fact]
    public void Backdrop_HasPresentationRole()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<NavigationMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<NavigationMenuBackdrop>());

        Assert.Equal("presentation", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void Backdrop_HiddenWhenInactive()
    {
        var ctx = CreateContext(activeValue: null);
        var cut = Render<CascadingValue<NavigationMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<NavigationMenuBackdrop>());

        Assert.True(cut.Find("div").HasAttribute("hidden"));
    }

    [Fact]
    public void Backdrop_NotHiddenWhenActive()
    {
        var ctx = CreateContext(activeValue: "products");
        var cut = Render<CascadingValue<NavigationMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<NavigationMenuBackdrop>());

        Assert.False(cut.Find("div").HasAttribute("hidden"));
    }

    [Fact]
    public void Backdrop_HasDataOpenWhenActive()
    {
        var ctx = CreateContext(activeValue: "products");
        var cut = Render<CascadingValue<NavigationMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<NavigationMenuBackdrop>());

        Assert.NotNull(cut.Find("[data-open]"));
    }

    [Fact]
    public void Backdrop_HasDataClosedWhenInactive()
    {
        var ctx = CreateContext(activeValue: null);
        var cut = Render<CascadingValue<NavigationMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<NavigationMenuBackdrop>());

        Assert.NotNull(cut.Find("[data-closed]"));
    }

    /// <summary>
    /// Renders a Content + Viewport pair under a shared cascading context.
    /// Content registers its fragment with the context; Viewport renders it.
    /// </summary>
    private Bunit.IRenderedComponent<CascadingValue<NavigationMenuContext>> RenderContentWithViewport(
        NavigationMenuContext ctx, string value, string childContent)
    {
        return Render<CascadingValue<NavigationMenuContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent(b =>
            {
                b.OpenComponent<NavigationMenuContent>(0);
                b.AddAttribute(1, "Value", value);
                b.AddAttribute(2, "ChildContent",
                    (RenderFragment)(c => c.AddContent(0, childContent)));
                b.CloseComponent();

                b.OpenComponent<NavigationMenuViewport>(3);
                b.CloseComponent();
            }));
    }
}
