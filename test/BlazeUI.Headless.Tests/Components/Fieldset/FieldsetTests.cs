using BlazeUI.Headless.Components.Fieldset;
using Bunit;

namespace BlazeUI.Headless.Tests.Components.Fieldset;

public class FieldsetTests : BunitContext
{
    [Fact]
    public void Root_RendersFieldsetElement()
    {
        var cut = Render<FieldsetRoot>();
        Assert.Equal("FIELDSET", cut.Find("fieldset").TagName);
    }

    [Fact]
    public void Root_Disabled_SetsNativeAndDataAttributes()
    {
        var cut = Render<FieldsetRoot>(p => p.Add(c => c.Disabled, true));
        var fieldset = cut.Find("fieldset");
        Assert.NotNull(fieldset.GetAttribute("disabled"));
        Assert.NotNull(fieldset.GetAttribute("data-disabled"));
    }

    [Fact]
    public void Root_NotDisabled_OmitsDisabledAttributes()
    {
        var cut = Render<FieldsetRoot>();
        var fieldset = cut.Find("fieldset");
        Assert.Null(fieldset.GetAttribute("disabled"));
        Assert.Null(fieldset.GetAttribute("data-disabled"));
    }

    // Base UI: FieldsetLegend renders as a <div>, not a native <legend>.
    [Fact]
    public void Legend_RendersAsDivElement()
    {
        var cut = Render<FieldsetRoot>(p => p
            .AddChildContent<FieldsetLegend>(lp => lp
                .AddChildContent("Personal Info")));

        Assert.Equal("Personal Info", cut.Find("div").TextContent);
        Assert.Empty(cut.FindAll("legend"));
    }

    // Core Base UI contract: when a legend is present, the fieldset gets aria-labelledby
    // pointing at the legend's auto-generated ID.
    [Fact]
    public void Root_WithLegend_SetsAriaLabelledby()
    {
        var cut = Render<FieldsetRoot>(p => p
            .AddChildContent<FieldsetLegend>(lp => lp
                .AddChildContent("Personal Info")));

        var fieldset = cut.Find("fieldset");
        var legend = cut.Find("div");

        var labelledBy = fieldset.GetAttribute("aria-labelledby");
        var legendId = legend.GetAttribute("id");

        Assert.NotNull(labelledBy);
        Assert.Equal(legendId, labelledBy);
    }

    // When a custom id is supplied to the legend, aria-labelledby uses that id.
    [Fact]
    public void Root_WithLegendCustomId_SetsAriaLabelledbyToCustomId()
    {
        var cut = Render<FieldsetRoot>(p => p
            .AddChildContent<FieldsetLegend>(lp => lp
                .Add(c => c.Id, "my-legend")
                .AddChildContent("Personal Info")));

        var fieldset = cut.Find("fieldset");
        Assert.Equal("my-legend", fieldset.GetAttribute("aria-labelledby"));
    }

    // Without a legend, aria-labelledby must not be present.
    [Fact]
    public void Root_WithoutLegend_NoAriaLabelledby()
    {
        var cut = Render<FieldsetRoot>();
        Assert.Null(cut.Find("fieldset").GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void Legend_InheritedDisabledState_SetsDataDisabled()
    {
        var cut = Render<FieldsetRoot>(p => p
            .Add(c => c.Disabled, true)
            .AddChildContent<FieldsetLegend>(lp => lp
                .AddChildContent("Personal Info")));

        Assert.NotNull(cut.Find("div[data-disabled]"));
    }
}
