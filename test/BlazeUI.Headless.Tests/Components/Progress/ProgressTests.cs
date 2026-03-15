using BlazeUI.Headless.Components.Progress;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Tests.Components.Progress;

public class ProgressTests : BunitContext
{
    // Helper: build a ProgressContext with a correctly computed Status.
    private static ProgressContext MakeContext(double? value, double min = 0, double max = 100)
    {
        ProgressStatus status = value is null
            ? ProgressStatus.Indeterminate
            : value.Value >= max ? ProgressStatus.Complete : ProgressStatus.Progressing;

        return new ProgressContext
        {
            Value = value,
            Min = min,
            Max = max,
            Status = status,
            FormattedValue = value.HasValue
                ? (value.Value / max).ToString("P", System.Globalization.CultureInfo.CurrentCulture)
                : "",
        };
    }

    // ── ProgressRoot ──────────────────────────────────────────────────────────

    [Fact]
    public void Root_RendersDefaultDivTag()
    {
        var cut = Render<ProgressRoot>(p => p.Add(c => c.Value, 50));
        Assert.Equal("DIV", cut.Find("[role='progressbar']").TagName);
    }

    [Fact]
    public void Root_HasProgressbarRole()
    {
        var cut = Render<ProgressRoot>(p => p.Add(c => c.Value, 50));
        Assert.Equal("progressbar", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void Root_SetsAriaAttributes()
    {
        var cut = Render<ProgressRoot>(p => p
            .Add(c => c.Value, 30)
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100));

        var el = cut.Find("[role='progressbar']");
        Assert.Equal("30", el.GetAttribute("aria-valuenow"));
        Assert.Equal("0", el.GetAttribute("aria-valuemin"));
        Assert.Equal("100", el.GetAttribute("aria-valuemax"));
    }

    [Fact]
    public void Root_IndeterminateOmitsAriaValuenow()
    {
        var cut = Render<ProgressRoot>();

        var el = cut.Find("[role='progressbar']");
        Assert.Null(el.GetAttribute("aria-valuenow"));
        Assert.NotNull(cut.Find("[data-indeterminate]"));
    }

    [Fact]
    public void Root_SetsAriaValuetextForIndeterminate()
    {
        var cut = Render<ProgressRoot>();
        Assert.Equal("indeterminate progress", cut.Find("[role='progressbar']").GetAttribute("aria-valuetext"));
    }

    [Fact]
    public void Root_DataProgressingWhenInProgress()
    {
        var cut = Render<ProgressRoot>(p => p.Add(c => c.Value, 50));

        Assert.NotNull(cut.Find("[data-progressing]"));
        Assert.Empty(cut.FindAll("[data-indeterminate]"));
        Assert.Empty(cut.FindAll("[data-complete]"));
    }

    [Fact]
    public void Root_DataCompleteWhenValueEqualsMax()
    {
        var cut = Render<ProgressRoot>(p => p
            .Add(c => c.Value, 100)
            .Add(c => c.Max, 100));

        Assert.NotNull(cut.Find("[data-complete]"));
        Assert.Empty(cut.FindAll("[data-progressing]"));
    }

    [Fact]
    public void Root_SetsCssCustomProperties()
    {
        var cut = Render<ProgressRoot>(p => p
            .Add(c => c.Value, 60)
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100));

        var style = cut.Find("[role='progressbar']").GetAttribute("style");
        Assert.Contains("--progress-value:60", style);
        Assert.Contains("--progress-min:0", style);
        Assert.Contains("--progress-max:100", style);
    }

    [Fact]
    public void Root_AriaLabelledByPointsAtLabel()
    {
        // The Root must wire aria-labelledby to the Label's generated ID.
        var cut = Render<ProgressRoot>(p => p
            .Add(c => c.Value, 50)
            .AddChildContent<ProgressLabel>(lp => lp.AddChildContent("Downloading")));

        var progressbar = cut.Find("[role='progressbar']");
        var label = cut.Find("[role='presentation']");

        var labelledBy = progressbar.GetAttribute("aria-labelledby");
        Assert.NotNull(labelledBy);
        Assert.Equal(label.Id, labelledBy);
    }

    // ── ProgressLabel ─────────────────────────────────────────────────────────

    [Fact]
    public void Label_RendersSpanWithRolePresentation()
    {
        var ctx = MakeContext(50);
        var cut = Render<CascadingValue<ProgressContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ProgressLabel>(np => np.AddChildContent("Loading")));

        var span = cut.Find("span");
        Assert.Equal("SPAN", span.TagName);
        Assert.Equal("presentation", span.GetAttribute("role"));
        Assert.Contains("Loading", cut.Markup);
    }

    [Fact]
    public void Label_HasAllThreeDataAttributes_WhenProgressing()
    {
        var ctx = MakeContext(50);
        var cut = Render<CascadingValue<ProgressContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ProgressLabel>());

        Assert.NotNull(cut.Find("[data-progressing]"));
        Assert.Empty(cut.FindAll("[data-indeterminate]"));
        Assert.Empty(cut.FindAll("[data-complete]"));
    }

    [Fact]
    public void Label_HasDataIndeterminate_WhenNull()
    {
        var ctx = MakeContext(null);
        var cut = Render<CascadingValue<ProgressContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ProgressLabel>());

        Assert.NotNull(cut.Find("[data-indeterminate]"));
    }

    // ── ProgressValue ─────────────────────────────────────────────────────────

    [Fact]
    public void Value_RendersSpanByDefault()
    {
        var ctx = MakeContext(60);
        var cut = Render<CascadingValue<ProgressContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ProgressValue>(np => np.AddChildContent("60%")));

        Assert.Equal("SPAN", cut.Find("span").TagName);
        Assert.Contains("60%", cut.Markup);
    }

    [Fact]
    public void Value_IsAriaHidden()
    {
        var ctx = MakeContext(50);
        var cut = Render<CascadingValue<ProgressContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ProgressValue>());

        Assert.Equal("true", cut.Find("span").GetAttribute("aria-hidden"));
    }

    [Fact]
    public void Value_RendersFormattedValueFromContext()
    {
        // FormattedValue is set by ProgressRoot; in unit tests we set it directly.
        var ctx = MakeContext(30);
        ctx.FormattedValue = "30%";

        var cut = Render<CascadingValue<ProgressContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ProgressValue>());

        Assert.Contains("30%", cut.Markup);
    }

    [Fact]
    public void Value_RendersNothingWhenIndeterminate()
    {
        var ctx = MakeContext(null);
        var cut = Render<CascadingValue<ProgressContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ProgressValue>());

        // The span exists but contains no text content.
        var span = cut.Find("span");
        Assert.Equal("", span.TextContent.Trim());
    }

    [Fact]
    public void Value_HasDataCompleteWhenComplete()
    {
        var ctx = MakeContext(100);
        var cut = Render<CascadingValue<ProgressContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ProgressValue>());

        Assert.NotNull(cut.Find("[data-complete]"));
    }

    [Fact]
    public void Value_HasDataIndeterminateWhenNull()
    {
        var ctx = MakeContext(null);
        var cut = Render<CascadingValue<ProgressContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ProgressValue>());

        Assert.NotNull(cut.Find("[data-indeterminate]"));
    }

    [Fact]
    public void Value_HasDataProgressingWhenInProgress()
    {
        var ctx = MakeContext(50);
        var cut = Render<CascadingValue<ProgressContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ProgressValue>());

        Assert.NotNull(cut.Find("[data-progressing]"));
    }

    // ── ProgressTrack ─────────────────────────────────────────────────────────

    [Fact]
    public void Track_RendersDefaultDivTag()
    {
        var ctx = MakeContext(50);
        var cut = Render<CascadingValue<ProgressContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ProgressTrack>());

        Assert.Equal("DIV", cut.Find("div").TagName);
    }

    [Fact]
    public void Track_HasDataAttributeMatchingStatus()
    {
        var ctx = MakeContext(50);
        var cut = Render<CascadingValue<ProgressContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ProgressTrack>());

        Assert.NotNull(cut.Find("[data-progressing]"));
    }

    // ── ProgressIndicator ─────────────────────────────────────────────────────

    [Fact]
    public void Indicator_RendersDefaultDivTag()
    {
        var ctx = MakeContext(50);
        var cut = Render<CascadingValue<ProgressContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ProgressIndicator>());

        Assert.Equal("DIV", cut.Find("div").TagName);
    }

    [Fact]
    public void Indicator_SetsWidthStyleWhenDeterminate()
    {
        var ctx = MakeContext(33);
        var cut = Render<CascadingValue<ProgressContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ProgressIndicator>());

        var style = cut.Find("div").GetAttribute("style");
        Assert.Contains("width: 33%", style);
        Assert.Contains("inset-inline-start: 0", style);
    }

    [Fact]
    public void Indicator_NoInlineStyleWhenIndeterminate()
    {
        var ctx = MakeContext(null);
        var cut = Render<CascadingValue<ProgressContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ProgressIndicator>());

        // No indicator-specific style should be present when indeterminate.
        var style = cut.Find("div").GetAttribute("style");
        Assert.True(string.IsNullOrEmpty(style));
    }

    [Fact]
    public void Indicator_HasDataAttributeMatchingStatus()
    {
        var ctx = MakeContext(null);
        var cut = Render<CascadingValue<ProgressContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<ProgressIndicator>());

        Assert.NotNull(cut.Find("[data-indeterminate]"));
    }
}
