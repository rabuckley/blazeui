using BlazeUI.Headless.Components.PreviewCard;
using BlazeUI.Headless.Core;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Headless.Tests.Components.PreviewCard;

public class PreviewCardTests : BunitContext
{
    public PreviewCardTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazeUI();
    }

    // Creates a context with known IDs for assertion purposes.
    private static PreviewCardContext CreateContext(
        bool open = false,
        string? titleId = null,
        string? descriptionId = null,
        string side = "bottom",
        string align = "center") =>
        new()
        {
            TriggerId = "test-trigger",
            PopupId = "test-popup",
            PositionerId = "test-positioner",
            Open = open,
            TitleId = titleId,
            DescriptionId = descriptionId,
            Side = side,
            Align = align,
        };

    // --- PreviewCardTrigger ---

    [Fact]
    public void Trigger_DefaultTagIsAnchor()
    {
        var ctx = CreateContext();

        var cut = Render(b =>
        {
            b.OpenComponent<CascadingValue<PreviewCardContext>>(0);
            b.AddComponentParameter(1, "Value", ctx);
            b.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<PreviewCardTrigger>(0);
                inner.CloseComponent();
            }));
            b.CloseComponent();
        });

        Assert.Equal("A", cut.Find("a").TagName);
    }

    [Fact]
    public void Trigger_DataPopupOpenAbsentWhenClosed()
    {
        var ctx = CreateContext(open: false);

        var cut = Render(b =>
        {
            b.OpenComponent<CascadingValue<PreviewCardContext>>(0);
            b.AddComponentParameter(1, "Value", ctx);
            b.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<PreviewCardTrigger>(0);
                inner.CloseComponent();
            }));
            b.CloseComponent();
        });

        Assert.Empty(cut.FindAll("[data-popup-open]"));
    }

    [Fact]
    public void Trigger_DataPopupOpenPresentWhenOpen()
    {
        var ctx = CreateContext(open: true);

        var cut = Render(b =>
        {
            b.OpenComponent<CascadingValue<PreviewCardContext>>(0);
            b.AddComponentParameter(1, "Value", ctx);
            b.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<PreviewCardTrigger>(0);
                inner.CloseComponent();
            }));
            b.CloseComponent();
        });

        Assert.NotNull(cut.Find("[data-popup-open]"));
    }

    // --- PreviewCardPopup ---

    [Fact]
    public void Popup_NotRenderedByDefault()
    {
        // Popup is unmounted when closed and never opened.
        var ctx = CreateContext(open: false);

        var cut = Render(b =>
        {
            b.OpenComponent<CascadingValue<PreviewCardContext>>(0);
            b.AddComponentParameter(1, "Value", ctx);
            b.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<PreviewCardPopup>(0);
                inner.CloseComponent();
            }));
            b.CloseComponent();
        });

        Assert.Empty(cut.FindAll("div"));
    }

    [Fact]
    public void Popup_MountedWhenOpen()
    {
        var ctx = CreateContext(open: true);

        var cut = Render(b =>
        {
            b.OpenComponent<CascadingValue<PreviewCardContext>>(0);
            b.AddComponentParameter(1, "Value", ctx);
            b.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<PreviewCardPopup>(0);
                inner.CloseComponent();
            }));
            b.CloseComponent();
        });

        Assert.NotNull(cut.Find("div"));
    }

    [Fact]
    public void Popup_HasCorrectId()
    {
        var ctx = CreateContext(open: true);

        var cut = Render(b =>
        {
            b.OpenComponent<CascadingValue<PreviewCardContext>>(0);
            b.AddComponentParameter(1, "Value", ctx);
            b.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<PreviewCardPopup>(0);
                inner.CloseComponent();
            }));
            b.CloseComponent();
        });

        Assert.Equal("test-popup", cut.Find("div").GetAttribute("id"));
    }

    // data-open/data-closed are owned by JS (not rendered by Blazor) to
    // avoid re-render cycles resetting animation state. Verified by E2E tests.

    [Fact]
    public void Popup_HasDataSideAndAlignFromContext()
    {
        var ctx = CreateContext(open: true, side: "top", align: "start");

        var cut = Render(b =>
        {
            b.OpenComponent<CascadingValue<PreviewCardContext>>(0);
            b.AddComponentParameter(1, "Value", ctx);
            b.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<PreviewCardPopup>(0);
                inner.CloseComponent();
            }));
            b.CloseComponent();
        });

        var popup = cut.Find("div");
        Assert.Equal("top", popup.GetAttribute("data-side"));
        Assert.Equal("start", popup.GetAttribute("data-align"));
    }

    [Fact]
    public void Popup_AriaLabelledByTitleId()
    {
        var ctx = CreateContext(open: true, titleId: "title-123");

        var cut = Render(b =>
        {
            b.OpenComponent<CascadingValue<PreviewCardContext>>(0);
            b.AddComponentParameter(1, "Value", ctx);
            b.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<PreviewCardPopup>(0);
                inner.CloseComponent();
            }));
            b.CloseComponent();
        });

        Assert.Equal("title-123", cut.Find("div").GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void Popup_AriaDescribedByDescriptionId()
    {
        var ctx = CreateContext(open: true, descriptionId: "desc-456");

        var cut = Render(b =>
        {
            b.OpenComponent<CascadingValue<PreviewCardContext>>(0);
            b.AddComponentParameter(1, "Value", ctx);
            b.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<PreviewCardPopup>(0);
                inner.AddComponentParameter(1, "DescriptionId", "desc-456");
                inner.CloseComponent();
            }));
            b.CloseComponent();
        });

        Assert.Equal("desc-456", cut.Find("div").GetAttribute("aria-describedby"));
    }

    // --- PreviewCardPositioner ---

    [Fact]
    public void Positioner_HasCorrectId()
    {
        var ctx = CreateContext();

        var cut = Render(b =>
        {
            b.OpenComponent<CascadingValue<PreviewCardContext>>(0);
            b.AddComponentParameter(1, "Value", ctx);
            b.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<PreviewCardPositioner>(0);
                inner.CloseComponent();
            }));
            b.CloseComponent();
        });

        Assert.Equal("test-positioner", cut.Find("div").GetAttribute("id"));
    }

    [Fact]
    public void Positioner_HasRolePresentation()
    {
        var ctx = CreateContext();

        var cut = Render(b =>
        {
            b.OpenComponent<CascadingValue<PreviewCardContext>>(0);
            b.AddComponentParameter(1, "Value", ctx);
            b.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<PreviewCardPositioner>(0);
                inner.CloseComponent();
            }));
            b.CloseComponent();
        });

        Assert.Equal("presentation", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void Positioner_DefaultDataSideIsBottom()
    {
        var ctx = CreateContext();

        var cut = Render(b =>
        {
            b.OpenComponent<CascadingValue<PreviewCardContext>>(0);
            b.AddComponentParameter(1, "Value", ctx);
            b.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<PreviewCardPositioner>(0);
                inner.CloseComponent();
            }));
            b.CloseComponent();
        });

        Assert.Equal("bottom", cut.Find("div").GetAttribute("data-side"));
    }

    [Fact]
    public void Positioner_DataSideReflectsParameter()
    {
        var ctx = CreateContext();

        var cut = Render(b =>
        {
            b.OpenComponent<CascadingValue<PreviewCardContext>>(0);
            b.AddComponentParameter(1, "Value", ctx);
            b.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<PreviewCardPositioner>(0);
                inner.AddComponentParameter(1, "Side", Side.Top);
                inner.CloseComponent();
            }));
            b.CloseComponent();
        });

        Assert.Equal("top", cut.Find("div").GetAttribute("data-side"));
    }

    // --- PreviewCardArrow ---

    [Fact]
    public void Arrow_IsAriaHidden()
    {
        var ctx = CreateContext(open: true);

        var cut = Render(b =>
        {
            b.OpenComponent<CascadingValue<PreviewCardContext>>(0);
            b.AddComponentParameter(1, "Value", ctx);
            b.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<PreviewCardArrow>(0);
                inner.CloseComponent();
            }));
            b.CloseComponent();
        });

        Assert.Equal("true", cut.Find("div").GetAttribute("aria-hidden"));
    }

    [Fact]
    public void Arrow_HasDataSideFromContext()
    {
        var ctx = CreateContext(open: true, side: "right");

        var cut = Render(b =>
        {
            b.OpenComponent<CascadingValue<PreviewCardContext>>(0);
            b.AddComponentParameter(1, "Value", ctx);
            b.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<PreviewCardArrow>(0);
                inner.CloseComponent();
            }));
            b.CloseComponent();
        });

        Assert.Equal("right", cut.Find("div").GetAttribute("data-side"));
    }

    // --- PreviewCardBackdrop ---

    [Fact]
    public void Backdrop_NotRenderedWhenClosed()
    {
        var ctx = CreateContext(open: false);

        var cut = Render(b =>
        {
            b.OpenComponent<CascadingValue<PreviewCardContext>>(0);
            b.AddComponentParameter(1, "Value", ctx);
            b.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<PreviewCardBackdrop>(0);
                inner.CloseComponent();
            }));
            b.CloseComponent();
        });

        Assert.Empty(cut.FindAll("div"));
    }

    [Fact]
    public void Backdrop_MountedWhenOpen()
    {
        var ctx = CreateContext(open: true);

        var cut = Render(b =>
        {
            b.OpenComponent<CascadingValue<PreviewCardContext>>(0);
            b.AddComponentParameter(1, "Value", ctx);
            b.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<PreviewCardBackdrop>(0);
                inner.CloseComponent();
            }));
            b.CloseComponent();
        });

        Assert.NotNull(cut.Find("div"));
    }

    [Fact]
    public void Backdrop_HasRolePresentation()
    {
        var ctx = CreateContext(open: true);

        var cut = Render(b =>
        {
            b.OpenComponent<CascadingValue<PreviewCardContext>>(0);
            b.AddComponentParameter(1, "Value", ctx);
            b.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<PreviewCardBackdrop>(0);
                inner.CloseComponent();
            }));
            b.CloseComponent();
        });

        Assert.Equal("presentation", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void Backdrop_HasDataOpenWhenOpen()
    {
        var ctx = CreateContext(open: true);

        var cut = Render(b =>
        {
            b.OpenComponent<CascadingValue<PreviewCardContext>>(0);
            b.AddComponentParameter(1, "Value", ctx);
            b.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<PreviewCardBackdrop>(0);
                inner.CloseComponent();
            }));
            b.CloseComponent();
        });

        Assert.NotNull(cut.Find("[data-open]"));
        Assert.Empty(cut.FindAll("[data-closed]"));
    }

    // --- PreviewCardRoot ---

    [Fact]
    public void Root_CascadesContext()
    {
        // Verify that PreviewCardRoot cascades context so children can receive it.
        var cut = Render<PreviewCardRoot>(p => p
            .AddChildContent<PreviewCardTrigger>(_ => { }));

        Assert.NotNull(cut.Find("a"));
    }

    [Fact]
    public void Root_DefaultOpenFalse()
    {
        // Trigger should render with no data-popup-open by default.
        var cut = Render<PreviewCardRoot>(p => p
            .AddChildContent<PreviewCardTrigger>(_ => { }));

        Assert.Empty(cut.FindAll("[data-popup-open]"));
    }

    [Fact]
    public void Root_DefaultOpenTrue_RendersOpenState()
    {
        var cut = Render<PreviewCardRoot>(p => p
            .Add(c => c.DefaultOpen, true)
            .AddChildContent<PreviewCardTrigger>(_ => { }));

        Assert.NotNull(cut.Find("[data-popup-open]"));
    }

    // --- PreviewCardViewport ---

    [Fact]
    public void Viewport_RendersDefaultDiv()
    {
        var ctx = CreateContext();

        var cut = Render(b =>
        {
            b.OpenComponent<CascadingValue<PreviewCardContext>>(0);
            b.AddComponentParameter(1, "Value", ctx);
            b.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<PreviewCardViewport>(0);
                inner.CloseComponent();
            }));
            b.CloseComponent();
        });

        Assert.Equal("DIV", cut.Find("div").TagName);
    }
}
