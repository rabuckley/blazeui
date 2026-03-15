using BlazeUI.Headless.Components.Toast;
using BlazeUI.Headless.Core;
using BlazeUI.Headless.Overlay.Toast;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Headless.Tests.Components.Toast;

public class ToastTests : BunitContext
{
    public ToastTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazeUI();
    }

    private static ToastContext CreateContext(
        string toastId = "toast-1",
        string? toastType = null,
        string? titleId = null,
        string? descriptionId = null,
        bool expanded = false,
        int visibleIndex = 0) =>
        new()
        {
            ToastId = toastId,
            ToastType = toastType,
            TitleId = titleId,
            DescriptionId = descriptionId,
            Expanded = expanded,
            VisibleIndex = visibleIndex,
        };

    // Wraps a render fragment in a ToastContext cascade.
    private Bunit.IRenderedComponent<Bunit.Rendering.ContainerFragment> WithContext(
        ToastContext ctx,
        RenderFragment content) =>
        Render(b =>
        {
            b.OpenComponent<CascadingValue<ToastContext>>(0);
            b.AddComponentParameter(1, "Value", ctx);
            b.AddComponentParameter(2, "ChildContent", content);
            b.CloseComponent();
        });

    // --- ToastViewport ---

    [Fact]
    public void Viewport_HasRegionRole()
    {
        var cut = Render<ToastViewport>();
        Assert.Equal("region", cut.Find("[role='region']").GetAttribute("role"));
    }

    [Fact]
    public void Viewport_HasAriaLivePolite()
    {
        var cut = Render<ToastViewport>();
        Assert.Equal("polite", cut.Find("[role='region']").GetAttribute("aria-live"));
    }

    [Fact]
    public void Viewport_HasAriaAtomicFalse()
    {
        var cut = Render<ToastViewport>();
        Assert.Equal("false", cut.Find("[role='region']").GetAttribute("aria-atomic"));
    }

    [Fact]
    public void Viewport_HasAccessibleLabel()
    {
        var cut = Render<ToastViewport>();
        var label = cut.Find("[role='region']").GetAttribute("aria-label");
        Assert.NotNull(label);
        Assert.NotEmpty(label);
    }

    [Fact]
    public void Viewport_HasTabIndexMinusOne()
    {
        var cut = Render<ToastViewport>();
        Assert.Equal("-1", cut.Find("[role='region']").GetAttribute("tabindex"));
    }

    [Fact]
    public void Viewport_DataExpandedAbsentWhenNotExpanded()
    {
        var cut = Render<ToastViewport>(p => p.Add(c => c.Expanded, false));
        Assert.Empty(cut.FindAll("[data-expanded]"));
    }

    [Fact]
    public void Viewport_DataExpandedPresentWhenExpanded()
    {
        var cut = Render<ToastViewport>(p => p.Add(c => c.Expanded, true));
        Assert.NotNull(cut.Find("[data-expanded]"));
    }

    // --- ToastRoot ---

    [Fact]
    public void Root_DefaultTagIsDiv()
    {
        var cut = Render<ToastRoot>(p => p.Add(c => c.ToastId, "test-id"));
        Assert.Equal("DIV", cut.Find("[role='dialog']").TagName);
    }

    [Fact]
    public void Root_HasDialogRole()
    {
        var cut = Render<ToastRoot>(p => p.Add(c => c.ToastId, "test-id"));
        Assert.Equal("dialog", cut.Find("[role='dialog']").GetAttribute("role"));
    }

    [Fact]
    public void Root_HasTabIndex0()
    {
        var cut = Render<ToastRoot>(p => p.Add(c => c.ToastId, "test-id"));
        Assert.Equal("0", cut.Find("[role='dialog']").GetAttribute("tabindex"));
    }

    [Fact]
    public void Root_HasAriaModalFalse()
    {
        var cut = Render<ToastRoot>(p => p.Add(c => c.ToastId, "test-id"));
        Assert.Equal("false", cut.Find("[role='dialog']").GetAttribute("aria-modal"));
    }

    [Fact]
    public void Root_EmitsDataTypeWhenToastTypeSet()
    {
        var cut = Render<ToastRoot>(p => p
            .Add(c => c.ToastId, "test-id")
            .Add(c => c.ToastType, "success"));
        Assert.Equal("success", cut.Find("[data-type]").GetAttribute("data-type"));
    }

    [Fact]
    public void Root_DataTypeAbsentWhenNotSet()
    {
        var cut = Render<ToastRoot>(p => p.Add(c => c.ToastId, "test-id"));
        Assert.Empty(cut.FindAll("[data-type]"));
    }

    [Fact]
    public void Root_WiresAriaLabelledByFromTitle()
    {
        // ToastTitle registers its ID with the cascaded context, then ToastRoot
        // re-renders and emits aria-labelledby pointing at the title.
        var cut = Render(b =>
        {
            b.OpenComponent<ToastRoot>(0);
            b.AddComponentParameter(1, "ToastId", "root-1");
            b.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<ToastTitle>(0);
                inner.AddAttribute(1, "id", "title-1");
                inner.CloseComponent();
            }));
            b.CloseComponent();
        });

        Assert.Equal("title-1", cut.Find("[role='dialog']").GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void Root_WiresAriaDescribedByFromDescription()
    {
        var cut = Render(b =>
        {
            b.OpenComponent<ToastRoot>(0);
            b.AddComponentParameter(1, "ToastId", "root-2");
            b.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<ToastDescription>(0);
                inner.AddAttribute(1, "id", "desc-1");
                inner.CloseComponent();
            }));
            b.CloseComponent();
        });

        Assert.Equal("desc-1", cut.Find("[role='dialog']").GetAttribute("aria-describedby"));
    }

    // --- ToastTitle ---

    [Fact]
    public void Title_DefaultTagIsH2()
    {
        var ctx = CreateContext();
        var cut = WithContext(ctx, inner =>
        {
            inner.OpenComponent<ToastTitle>(0);
            inner.CloseComponent();
        });

        Assert.Equal("H2", cut.Find("h2").TagName);
    }

    [Fact]
    public void Title_EmitsDataTypeWhenTypeSet()
    {
        var ctx = CreateContext(toastType: "error");
        var cut = WithContext(ctx, inner =>
        {
            inner.OpenComponent<ToastTitle>(0);
            inner.CloseComponent();
        });

        Assert.Equal("error", cut.Find("[data-type]").GetAttribute("data-type"));
    }

    [Fact]
    public void Title_NoDataTypeWhenNotSet()
    {
        var ctx = CreateContext();
        var cut = WithContext(ctx, inner =>
        {
            inner.OpenComponent<ToastTitle>(0);
            inner.CloseComponent();
        });

        Assert.Empty(cut.FindAll("[data-type]"));
    }

    // --- ToastDescription ---

    [Fact]
    public void Description_DefaultTagIsP()
    {
        var ctx = CreateContext();
        var cut = WithContext(ctx, inner =>
        {
            inner.OpenComponent<ToastDescription>(0);
            inner.CloseComponent();
        });

        Assert.Equal("P", cut.Find("p").TagName);
    }

    [Fact]
    public void Description_EmitsDataTypeWhenTypeSet()
    {
        var ctx = CreateContext(toastType: "warning");
        var cut = WithContext(ctx, inner =>
        {
            inner.OpenComponent<ToastDescription>(0);
            inner.CloseComponent();
        });

        Assert.Equal("warning", cut.Find("[data-type]").GetAttribute("data-type"));
    }

    // --- ToastClose ---

    [Fact]
    public void Close_DefaultTagIsButton()
    {
        var ctx = CreateContext();
        var cut = WithContext(ctx, inner =>
        {
            inner.OpenComponent<ToastClose>(0);
            inner.CloseComponent();
        });

        Assert.Equal("BUTTON", cut.Find("button").TagName);
    }

    [Fact]
    public void Close_EmitsDataTypeWhenTypeSet()
    {
        var ctx = CreateContext(toastType: "info");
        var cut = WithContext(ctx, inner =>
        {
            inner.OpenComponent<ToastClose>(0);
            inner.CloseComponent();
        });

        Assert.Equal("info", cut.Find("[data-type]").GetAttribute("data-type"));
    }

    // --- ToastAction ---

    [Fact]
    public void Action_DefaultTagIsButton()
    {
        var ctx = CreateContext();
        var cut = WithContext(ctx, inner =>
        {
            inner.OpenComponent<ToastAction>(0);
            inner.CloseComponent();
        });

        Assert.Equal("BUTTON", cut.Find("button").TagName);
    }

    [Fact]
    public void Action_EmitsDataTypeWhenTypeSet()
    {
        var ctx = CreateContext(toastType: "success");
        var cut = WithContext(ctx, inner =>
        {
            inner.OpenComponent<ToastAction>(0);
            inner.CloseComponent();
        });

        Assert.Equal("success", cut.Find("[data-type]").GetAttribute("data-type"));
    }

    // --- ToastContent ---

    [Fact]
    public void Content_DefaultTagIsDiv()
    {
        var ctx = CreateContext();
        var cut = WithContext(ctx, inner =>
        {
            inner.OpenComponent<ToastContent>(0);
            inner.CloseComponent();
        });

        Assert.Equal("DIV", cut.Find("div").TagName);
    }

    [Fact]
    public void Content_DataExpandedAbsentWhenNotExpanded()
    {
        var ctx = CreateContext(expanded: false);
        var cut = WithContext(ctx, inner =>
        {
            inner.OpenComponent<ToastContent>(0);
            inner.CloseComponent();
        });

        Assert.Empty(cut.FindAll("[data-expanded]"));
    }

    [Fact]
    public void Content_DataExpandedPresentWhenExpanded()
    {
        var ctx = CreateContext(expanded: true);
        var cut = WithContext(ctx, inner =>
        {
            inner.OpenComponent<ToastContent>(0);
            inner.CloseComponent();
        });

        Assert.NotNull(cut.Find("[data-expanded]"));
    }

    [Fact]
    public void Content_DataBehindAbsentWhenFrontmost()
    {
        var ctx = CreateContext(visibleIndex: 0);
        var cut = WithContext(ctx, inner =>
        {
            inner.OpenComponent<ToastContent>(0);
            inner.CloseComponent();
        });

        Assert.Empty(cut.FindAll("[data-behind]"));
    }

    [Fact]
    public void Content_DataBehindPresentWhenBehind()
    {
        var ctx = CreateContext(visibleIndex: 1);
        var cut = WithContext(ctx, inner =>
        {
            inner.OpenComponent<ToastContent>(0);
            inner.CloseComponent();
        });

        Assert.NotNull(cut.Find("[data-behind]"));
    }

    // --- ToastPositioner ---

    [Fact]
    public void Positioner_DefaultTagIsDiv()
    {
        var cut = Render<ToastPositioner>();
        Assert.Equal("DIV", cut.Find("[role='presentation']").TagName);
    }

    [Fact]
    public void Positioner_HasPresentationRole()
    {
        var cut = Render<ToastPositioner>();
        Assert.Equal("presentation", cut.Find("[role='presentation']").GetAttribute("role"));
    }

    [Fact]
    public void Positioner_EmitsDataSideAndAlign()
    {
        var cut = Render<ToastPositioner>(p => p
            .Add(c => c.Side, Side.Bottom)
            .Add(c => c.Align, Align.Start));
        var el = cut.Find("[role='presentation']");
        Assert.Equal("bottom", el.GetAttribute("data-side"));
        Assert.Equal("start", el.GetAttribute("data-align"));
    }

    // --- ToastArrow ---

    [Fact]
    public void Arrow_DefaultTagIsDiv()
    {
        var cut = Render<ToastArrow>();
        Assert.Equal("DIV", cut.Find("div").TagName);
    }

    [Fact]
    public void Arrow_IsAriaHidden()
    {
        var cut = Render<ToastArrow>();
        Assert.Equal("true", cut.Find("div").GetAttribute("aria-hidden"));
    }

    [Fact]
    public void Arrow_EmitsDataSideAndDataAlign()
    {
        var cut = Render<ToastArrow>(p => p
            .Add(c => c.Side, "bottom")
            .Add(c => c.Align, "start"));
        var el = cut.Find("div");
        Assert.Equal("bottom", el.GetAttribute("data-side"));
        Assert.Equal("start", el.GetAttribute("data-align"));
    }

    [Fact]
    public void Arrow_DataUncenteredAbsentByDefault()
    {
        var cut = Render<ToastArrow>();
        Assert.Empty(cut.FindAll("[data-uncentered]"));
    }

    [Fact]
    public void Arrow_DataUncenteredPresentWhenSet()
    {
        var cut = Render<ToastArrow>(p => p.Add(c => c.Uncentered, true));
        Assert.NotNull(cut.Find("[data-uncentered]"));
    }
}
