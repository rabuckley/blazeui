using BlazeUI.Headless.Components.NumberField;
using BlazeUI.Headless.Core;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Headless.Tests.Components.NumberField;

public class NumberFieldScrubAreaTests : BunitContext
{
    public NumberFieldScrubAreaTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazeUI();
    }

    private static NumberFieldContext CreateContext(bool disabled = false, bool readOnly = false) => new()
    {
        InputId = "test-input",
        Disabled = disabled,
        ReadOnly = readOnly,
        Step = 1,
        LargeStep = 10,
        // Wire no-op delegates so the context is self-consistent.
        SetScrubbing = () => { },
        ClearScrubbing = () => { },
    };

    // -- NumberFieldScrubArea --

    [Fact]
    public void ScrubArea_RendersSpanByDefault()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<NumberFieldContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<NumberFieldScrubArea>(sp => sp.AddChildContent("Scrub")));

        Assert.Equal("SPAN", cut.Find("span").TagName);
    }

    [Fact]
    public void ScrubArea_HasPresentationRole()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<NumberFieldContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<NumberFieldScrubArea>(sp => sp.AddChildContent("Scrub")));

        Assert.Equal("presentation", cut.Find("span").GetAttribute("role"));
    }

    [Fact]
    public void ScrubArea_HasTouchActionNone()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<NumberFieldContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<NumberFieldScrubArea>(sp => sp.AddChildContent("Scrub")));

        var style = cut.Find("span").GetAttribute("style");
        Assert.Contains("touch-action: none", style!);
    }

    [Fact]
    public void ScrubArea_DisabledSetsDataAttribute()
    {
        var ctx = CreateContext(disabled: true);
        var cut = Render<CascadingValue<NumberFieldContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<NumberFieldScrubArea>(sp => sp.AddChildContent("Scrub")));

        Assert.NotNull(cut.Find("[data-disabled]"));
    }

    [Fact]
    public void ScrubArea_ReadOnlySetsDataAttribute()
    {
        var ctx = CreateContext(readOnly: true);
        var cut = Render<CascadingValue<NumberFieldContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<NumberFieldScrubArea>(sp => sp.AddChildContent("Scrub")));

        Assert.NotNull(cut.Find("[data-readonly]"));
    }

    [Fact]
    public void ScrubArea_NotScrubbingByDefault()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<NumberFieldContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<NumberFieldScrubArea>(sp => sp.AddChildContent("Scrub")));

        Assert.Empty(cut.FindAll("[data-scrubbing]"));
    }

    // -- NumberFieldScrubAreaCursor --

    [Fact]
    public void ScrubAreaCursor_NotRenderedWhenNotScrubbing()
    {
        var ctx = CreateContext();
        var scrubCtx = new NumberFieldScrubAreaContext
        {
            IsScrubbing = false,
            ScrubAreaId = "test-scrub",
        };

        var cut = Render(builder =>
        {
            builder.OpenComponent<CascadingValue<NumberFieldContext>>(0);
            builder.AddComponentParameter(1, "Value", ctx);
            builder.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<CascadingValue<NumberFieldScrubAreaContext>>(0);
                inner.AddComponentParameter(1, "Value", scrubCtx);
                inner.AddComponentParameter(2, "ChildContent", (RenderFragment)(c =>
                {
                    c.OpenComponent<NumberFieldScrubAreaCursor>(0);
                    c.AddComponentParameter(1, "ChildContent", (RenderFragment)(cursor =>
                        cursor.AddContent(0, "⬤")));
                    c.CloseComponent();
                }));
                inner.CloseComponent();
            }));
            builder.CloseComponent();
        });

        // Cursor should not render when not scrubbing.
        Assert.DoesNotContain("⬤", cut.Markup);
    }

    [Fact]
    public void ScrubAreaCursor_RenderedWhenScrubbing()
    {
        var ctx = CreateContext();
        var scrubCtx = new NumberFieldScrubAreaContext
        {
            IsScrubbing = true,
            ScrubAreaId = "test-scrub",
        };

        var cut = Render(builder =>
        {
            builder.OpenComponent<CascadingValue<NumberFieldContext>>(0);
            builder.AddComponentParameter(1, "Value", ctx);
            builder.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<CascadingValue<NumberFieldScrubAreaContext>>(0);
                inner.AddComponentParameter(1, "Value", scrubCtx);
                inner.AddComponentParameter(2, "ChildContent", (RenderFragment)(c =>
                {
                    c.OpenComponent<NumberFieldScrubAreaCursor>(0);
                    c.AddComponentParameter(1, "ChildContent", (RenderFragment)(cursor =>
                        cursor.AddContent(0, "⬤")));
                    c.CloseComponent();
                }));
                inner.CloseComponent();
            }));
            builder.CloseComponent();
        });

        Assert.Contains("⬤", cut.Markup);
    }

    [Fact]
    public void ScrubAreaCursor_HasFixedPositioning()
    {
        var ctx = CreateContext();
        var scrubCtx = new NumberFieldScrubAreaContext
        {
            IsScrubbing = true,
            ScrubAreaId = "test-scrub",
        };

        var cut = Render(builder =>
        {
            builder.OpenComponent<CascadingValue<NumberFieldContext>>(0);
            builder.AddComponentParameter(1, "Value", ctx);
            builder.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<CascadingValue<NumberFieldScrubAreaContext>>(0);
                inner.AddComponentParameter(1, "Value", scrubCtx);
                inner.AddComponentParameter(2, "ChildContent", (RenderFragment)(c =>
                {
                    c.OpenComponent<NumberFieldScrubAreaCursor>(0);
                    c.AddComponentParameter(1, "ChildContent", (RenderFragment)(cursor =>
                        cursor.AddContent(0, "⬤")));
                    c.CloseComponent();
                }));
                inner.CloseComponent();
            }));
            builder.CloseComponent();
        });

        var style = cut.Find("span").GetAttribute("style");
        Assert.Contains("position: fixed", style!);
    }

    [Fact]
    public void ScrubAreaCursor_HasPresentationRole()
    {
        var ctx = CreateContext();
        var scrubCtx = new NumberFieldScrubAreaContext
        {
            IsScrubbing = true,
            ScrubAreaId = "test-scrub",
        };

        var cut = Render(builder =>
        {
            builder.OpenComponent<CascadingValue<NumberFieldContext>>(0);
            builder.AddComponentParameter(1, "Value", ctx);
            builder.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<CascadingValue<NumberFieldScrubAreaContext>>(0);
                inner.AddComponentParameter(1, "Value", scrubCtx);
                inner.AddComponentParameter(2, "ChildContent", (RenderFragment)(c =>
                {
                    c.OpenComponent<NumberFieldScrubAreaCursor>(0);
                    c.AddComponentParameter(1, "ChildContent", (RenderFragment)(cursor =>
                        cursor.AddContent(0, "⬤")));
                    c.CloseComponent();
                }));
                inner.CloseComponent();
            }));
            builder.CloseComponent();
        });

        Assert.Equal("presentation", cut.Find("span").GetAttribute("role"));
    }
}
