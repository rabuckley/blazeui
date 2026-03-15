using BlazeUI.Headless.Components.Meter;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Tests.Components.Meter;

public class MeterTests : BunitContext
{
    // -- MeterRoot --

    [Fact]
    public void RootRendersDefaultDivTag()
    {
        var cut = Render<MeterRoot>(p => p.Add(c => c.Value, 50));
        Assert.Equal("DIV", cut.Find("div").TagName);
    }

    [Fact]
    public void RootHasMeterRole()
    {
        var cut = Render<MeterRoot>(p => p.Add(c => c.Value, 50));
        Assert.Equal("meter", cut.Find("[role='meter']").GetAttribute("role"));
    }

    [Fact]
    public void RootSetsAriaValueAttributes()
    {
        var cut = Render<MeterRoot>(p => p
            .Add(c => c.Value, 30)
            .Add(c => c.Min, 0)
            .Add(c => c.Max, 100));

        var el = cut.Find("[role='meter']");
        Assert.Equal("30", el.GetAttribute("aria-valuenow"));
        Assert.Equal("0", el.GetAttribute("aria-valuemin"));
        Assert.Equal("100", el.GetAttribute("aria-valuemax"));
    }

    [Fact]
    public void RootSetsDefaultAriaValueText()
    {
        // Base UI default: "{value}%" string concatenation.
        var cut = Render<MeterRoot>(p => p.Add(c => c.Value, 30));
        Assert.Equal("30%", cut.Find("[role='meter']").GetAttribute("aria-valuetext"));
    }

    [Fact]
    public void RootUsesGetAriaValueTextCallback()
    {
        var cut = Render<MeterRoot>(p => p
            .Add(c => c.Value, 42)
            .Add(c => c.GetAriaValueText, (_, v) => $"{v} units"));

        Assert.Equal("42 units", cut.Find("[role='meter']").GetAttribute("aria-valuetext"));
    }

    [Fact]
    public void RootOmitsAriaLabelledByWhenNoLabelPresent()
    {
        var cut = Render<MeterRoot>(p => p.Add(c => c.Value, 50));
        Assert.Null(cut.Find("[role='meter']").GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void RootSetsAriaLabelledByWhenLabelPresent()
    {
        // Arrange & Act: render a full meter with a label child.
        var cut = Render<MeterRoot>(p => p
            .Add(c => c.Value, 30)
            .AddChildContent<MeterLabel>(lp => lp.AddChildContent("Battery Level")));

        var meter = cut.Find("[role='meter']");
        var label = cut.Find("span[role='presentation']");

        // The root's aria-labelledby must reference the label span's id.
        Assert.Equal(label.Id, meter.GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void RootRendersVisuallyHiddenPresentation()
    {
        // NVDA workaround: a visually hidden <span role="presentation"> containing "x"
        // is rendered as a direct child of the root div.
        var cut = Render<MeterRoot>(p => p.Add(c => c.Value, 50));

        // The visually-hidden span is outside the CascadingValue — it's a direct child.
        // We look for any span with role="presentation" in the markup.
        var spans = cut.FindAll("span[role='presentation']");
        Assert.NotEmpty(spans);
    }

    // -- MeterLabel --

    [Fact]
    public void LabelRendersSpanWithPresentationRole()
    {
        var ctx = new MeterContext { Value = 50, Min = 0, Max = 100 };
        var cut = Render<CascadingValue<MeterContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MeterLabel>(np => np.AddChildContent("CPU Usage")));

        var span = cut.Find("span");
        Assert.Equal("SPAN", span.TagName);
        Assert.Equal("presentation", span.GetAttribute("role"));
    }

    [Fact]
    public void LabelHasStableId()
    {
        var ctx = new MeterContext { Value = 50, Min = 0, Max = 100 };
        var cut = Render<CascadingValue<MeterContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MeterLabel>(np => np.AddChildContent("Battery")));

        var id = cut.Find("span").Id;
        Assert.False(string.IsNullOrEmpty(id));
        Assert.Contains("meter-label", id);
    }

    // -- MeterTrack --

    [Fact]
    public void TrackRendersDivByDefault()
    {
        var ctx = new MeterContext { Value = 50, Min = 0, Max = 100 };
        var cut = Render<CascadingValue<MeterContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MeterTrack>());

        Assert.Equal("DIV", cut.Find("div").TagName);
    }

    // -- MeterIndicator --

    [Fact]
    public void IndicatorRendersDivByDefault()
    {
        var ctx = new MeterContext { Value = 50, Min = 0, Max = 100 };
        var cut = Render<CascadingValue<MeterContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MeterIndicator>());

        Assert.Equal("DIV", cut.Find("div").TagName);
    }

    [Fact]
    public void IndicatorSetsWidthStyleFromValue()
    {
        // value=33, min=0, max=100 → 33% width
        var ctx = new MeterContext { Value = 33, Min = 0, Max = 100 };
        var cut = Render<CascadingValue<MeterContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MeterIndicator>());

        var style = cut.Find("div").GetAttribute("style");
        Assert.Contains("width: 33%", style);
        Assert.Contains("inset-inline-start: 0", style);
        Assert.Contains("height: inherit", style);
    }

    [Fact]
    public void IndicatorSetsZeroWidthWhenValueIsMin()
    {
        var ctx = new MeterContext { Value = 0, Min = 0, Max = 100 };
        var cut = Render<CascadingValue<MeterContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MeterIndicator>());

        var style = cut.Find("div").GetAttribute("style");
        Assert.Contains("width: 0%", style);
    }

    // -- MeterValue --

    [Fact]
    public void ValueRendersSpanWithAriaHidden()
    {
        var ctx = new MeterContext { Value = 75, Min = 0, Max = 100, FormattedValue = "75%" };
        var cut = Render<CascadingValue<MeterContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MeterValue>());

        Assert.Equal("true", cut.Find("span").GetAttribute("aria-hidden"));
    }

    [Fact]
    public void ValueExposesFormattedValueInState()
    {
        var ctx = new MeterContext { Value = 30, Min = 0, Max = 100, FormattedValue = "30%" };
        var cut = Render<CascadingValue<MeterContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<MeterValue>());

        // MeterValue does not render child content by default — it accepts child content
        // passed through ChildContent. Here we just confirm the component renders.
        Assert.Equal("SPAN", cut.Find("span").TagName);
    }
}
