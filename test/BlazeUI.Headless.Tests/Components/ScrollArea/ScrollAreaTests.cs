using BlazeUI.Headless.Components.ScrollArea;
using BlazeUI.Headless.Core;
using Bunit;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Headless.Tests.Components.ScrollArea;

public class ScrollAreaTests : BunitContext
{
    public ScrollAreaTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazeUI();
    }

    // ----------------------------------------------------------------
    // Root
    // ----------------------------------------------------------------

    [Fact]
    public void Root_RendersDiv()
    {
        var cut = Render<ScrollAreaRoot>();
        Assert.Equal("DIV", cut.Find("div").TagName);
    }

    [Fact]
    public void Root_HasRolePresentationAttribute()
    {
        var cut = Render<ScrollAreaRoot>();
        Assert.Equal("presentation", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void Root_HasPositionRelativeStyle()
    {
        var cut = Render<ScrollAreaRoot>();
        var style = cut.Find("div").GetAttribute("style");
        Assert.Contains("position: relative", style);
    }

    [Fact]
    public void Root_HasCornerCssVarsInStyle()
    {
        var cut = Render<ScrollAreaRoot>();
        var style = cut.Find("div").GetAttribute("style");
        Assert.Contains("--scroll-area-corner-height", style);
        Assert.Contains("--scroll-area-corner-width", style);
    }

    // ----------------------------------------------------------------
    // Viewport
    // ----------------------------------------------------------------

    [Fact]
    public void Viewport_RendersDiv()
    {
        var cut = Render<ScrollAreaRoot>(p => p
            .AddChildContent<ScrollAreaViewport>());
        Assert.NotNull(cut.Find("div div"));
    }

    [Fact]
    public void Viewport_HasRolePresentationAttribute()
    {
        var cut = Render<ScrollAreaRoot>(p => p
            .AddChildContent<ScrollAreaViewport>());
        // Both root and viewport have role=presentation; find the viewport by its overflow style.
        var allDivs = cut.FindAll("div");
        var viewportDiv = allDivs.FirstOrDefault(d => d.GetAttribute("style")?.Contains("overflow: scroll") == true);
        Assert.NotNull(viewportDiv);
        Assert.Equal("presentation", viewportDiv.GetAttribute("role"));
    }

    [Fact]
    public void Viewport_HasOverflowScrollStyle()
    {
        var cut = Render<ScrollAreaRoot>(p => p
            .AddChildContent<ScrollAreaViewport>());
        var allDivs = cut.FindAll("div");
        var viewportDiv = allDivs.FirstOrDefault(d => d.GetAttribute("style")?.Contains("overflow: scroll") == true);
        Assert.NotNull(viewportDiv);
    }

    [Fact]
    public void Viewport_HasTabIndex0()
    {
        var cut = Render<ScrollAreaRoot>(p => p
            .AddChildContent<ScrollAreaViewport>());
        var allDivs = cut.FindAll("div");
        var viewportDiv = allDivs.FirstOrDefault(d => d.GetAttribute("style")?.Contains("overflow: scroll") == true);
        Assert.NotNull(viewportDiv);
        Assert.Equal("0", viewportDiv.GetAttribute("tabindex"));
    }

    // ----------------------------------------------------------------
    // Scrollbar
    // ----------------------------------------------------------------

    [Fact]
    public void Scrollbar_DefaultOrientationIsVertical()
    {
        var cut = Render<ScrollAreaRoot>(p => p
            .AddChildContent<ScrollAreaScrollbar>());
        Assert.Equal("vertical", cut.Find("[data-orientation]").GetAttribute("data-orientation"));
    }

    [Fact]
    public void Scrollbar_HorizontalOrientationAttribute()
    {
        var cut = Render<ScrollAreaRoot>(p => p
            .AddChildContent<ScrollAreaScrollbar>(sb => sb
                .Add(c => c.Orientation, Orientation.Horizontal)));
        Assert.Equal("horizontal", cut.Find("[data-orientation]").GetAttribute("data-orientation"));
    }

    [Fact]
    public void Scrollbar_HasAbsolutePositionStyle()
    {
        var cut = Render<ScrollAreaRoot>(p => p
            .AddChildContent<ScrollAreaScrollbar>());
        var scrollbar = cut.Find("[data-orientation]");
        Assert.Contains("position: absolute", scrollbar.GetAttribute("style"));
    }

    [Fact]
    public void Scrollbar_VerticalInsetInlineEnd()
    {
        var cut = Render<ScrollAreaRoot>(p => p
            .AddChildContent<ScrollAreaScrollbar>());
        var scrollbar = cut.Find("[data-orientation='vertical']");
        Assert.Contains("inset-inline-end: 0", scrollbar.GetAttribute("style"));
    }

    [Fact]
    public void Scrollbar_HorizontalInsetInlineStart()
    {
        var cut = Render<ScrollAreaRoot>(p => p
            .AddChildContent<ScrollAreaScrollbar>(sb => sb
                .Add(c => c.Orientation, Orientation.Horizontal)));
        var scrollbar = cut.Find("[data-orientation='horizontal']");
        Assert.Contains("inset-inline-start: 0", scrollbar.GetAttribute("style"));
    }

    [Fact]
    public void Scrollbar_DefaultVisibilityIsAlways()
    {
        var cut = Render<ScrollAreaRoot>(p => p
            .AddChildContent<ScrollAreaScrollbar>());
        var scrollbar = cut.Find("[data-orientation]");
        Assert.Equal("always", scrollbar.GetAttribute("data-visibility"));
    }

    [Fact]
    public void Scrollbar_AlwaysVisibilityAttribute()
    {
        var cut = Render<ScrollAreaRoot>(p => p
            .AddChildContent<ScrollAreaScrollbar>(sb => sb
                .Add(c => c.Visibility, ScrollbarVisibility.Always)));
        var scrollbar = cut.Find("[data-orientation]");
        Assert.Equal("always", scrollbar.GetAttribute("data-visibility"));
    }

    [Fact]
    public void Scrollbar_HoverVisibilityAttribute()
    {
        var cut = Render<ScrollAreaRoot>(p => p
            .AddChildContent<ScrollAreaScrollbar>(sb => sb
                .Add(c => c.Visibility, ScrollbarVisibility.Hover)));
        var scrollbar = cut.Find("[data-orientation]");
        Assert.Equal("hover", scrollbar.GetAttribute("data-visibility"));
    }

    [Fact]
    public void Scrollbar_ScrollVisibilityAttribute()
    {
        var cut = Render<ScrollAreaRoot>(p => p
            .AddChildContent<ScrollAreaScrollbar>(sb => sb
                .Add(c => c.Visibility, ScrollbarVisibility.Scroll)));
        var scrollbar = cut.Find("[data-orientation]");
        Assert.Equal("scroll", scrollbar.GetAttribute("data-visibility"));
    }

    [Fact]
    public void Scrollbar_AutoVisibilityMapsToHover()
    {
        var cut = Render<ScrollAreaRoot>(p => p
            .AddChildContent<ScrollAreaScrollbar>(sb => sb
                .Add(c => c.Visibility, ScrollbarVisibility.Auto)));
        var scrollbar = cut.Find("[data-orientation]");
        Assert.Equal("hover", scrollbar.GetAttribute("data-visibility"));
    }

    // ----------------------------------------------------------------
    // Thumb
    // ----------------------------------------------------------------

    [Fact]
    public void Thumb_HasDataOrientationVertical()
    {
        var cut = Render<ScrollAreaRoot>(p => p
            .AddChildContent<ScrollAreaScrollbar>(sb => sb
                .AddChildContent<ScrollAreaThumb>()));
        Assert.Equal("vertical", cut.Find("[data-orientation]").GetAttribute("data-orientation"));
    }

    [Fact]
    public void Thumb_HasDataOrientationHorizontal()
    {
        var cut = Render<ScrollAreaRoot>(p => p
            .AddChildContent<ScrollAreaScrollbar>(sb => sb
                .Add(c => c.Orientation, Orientation.Horizontal)
                .AddChildContent<ScrollAreaThumb>()));
        // Two data-orientation elements: one on scrollbar, one on thumb — both should say horizontal
        var elements = cut.FindAll("[data-orientation='horizontal']");
        Assert.NotEmpty(elements);
    }

    [Fact]
    public void Thumb_VerticalUsesHeightCssVar()
    {
        var cut = Render<ScrollAreaRoot>(p => p
            .AddChildContent<ScrollAreaScrollbar>(sb => sb
                .AddChildContent<ScrollAreaThumb>()));
        // Thumb is nested inside scrollbar — the scrollbar is data-orientation=vertical,
        // the thumb is also data-orientation=vertical. Find the innermost one.
        var thumb = cut.FindAll("[data-orientation='vertical']").Last();
        Assert.Contains("--scroll-area-thumb-height", thumb.GetAttribute("style"));
    }

    [Fact]
    public void Thumb_HorizontalUsesWidthCssVar()
    {
        var cut = Render<ScrollAreaRoot>(p => p
            .AddChildContent<ScrollAreaScrollbar>(sb => sb
                .Add(c => c.Orientation, Orientation.Horizontal)
                .AddChildContent<ScrollAreaThumb>()));
        var thumb = cut.FindAll("[data-orientation='horizontal']").Last();
        Assert.Contains("--scroll-area-thumb-width", thumb.GetAttribute("style"));
    }

    // ----------------------------------------------------------------
    // Corner
    // ----------------------------------------------------------------

    [Fact]
    public void Corner_RendersDiv()
    {
        var cut = Render<ScrollAreaRoot>(p => p
            .AddChildContent<ScrollAreaCorner>());
        var corner = cut.Find("[data-id$='-corner']");
        Assert.Equal("DIV", corner.TagName);
    }

    [Fact]
    public void Corner_HasAbsolutePositionStyle()
    {
        var cut = Render<ScrollAreaRoot>(p => p
            .AddChildContent<ScrollAreaCorner>());
        var corner = cut.Find("[data-id$='-corner']");
        Assert.Contains("position: absolute", corner.GetAttribute("style"));
    }

    // ----------------------------------------------------------------
    // Content
    // ----------------------------------------------------------------

    [Fact]
    public void Content_RendersDiv()
    {
        var cut = Render<ScrollAreaRoot>(p => p
            .AddChildContent<ScrollAreaViewport>(vp => vp
                .AddChildContent<ScrollAreaContent>()));
        Assert.NotNull(cut.Find("[role='presentation'][style*='min-width: fit-content']"));
    }

    [Fact]
    public void Content_HasMinWidthFitContent()
    {
        var cut = Render<ScrollAreaRoot>(p => p
            .AddChildContent<ScrollAreaViewport>(vp => vp
                .AddChildContent<ScrollAreaContent>()));
        var content = cut.Find("[style*='min-width: fit-content']");
        Assert.NotNull(content);
    }
}
