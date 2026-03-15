using BlazeUI.Headless.Components.Slider;
using BlazeUI.Headless.Core;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Headless.Tests.Components.Slider;

public class SliderTests : BunitContext
{
    public SliderTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazeUI();
    }

    // Renders a minimal slider with one thumb and the full sub-part hierarchy.
    private IRenderedComponent<SliderRoot> RenderSlider(
        Action<ComponentParameterCollectionBuilder<SliderRoot>>? configure = null)
    {
        return Render<SliderRoot>(p =>
        {
            configure?.Invoke(p);
            p.AddChildContent<SliderControl>(cp => cp
                .AddChildContent<SliderTrack>(tp => tp
                    .AddChildContent<SliderIndicator>()
                    .AddChildContent<SliderThumb>()));
        });
    }

    private IRenderedComponent<SliderRoot> RenderRangeSlider(
        Action<ComponentParameterCollectionBuilder<SliderRoot>>? configure = null)
    {
        return Render<SliderRoot>(p =>
        {
            configure?.Invoke(p);
            p.AddChildContent<SliderControl>(cp => cp
                .AddChildContent<SliderTrack>(tp => tp
                    .AddChildContent<SliderIndicator>()
                    .AddChildContent<SliderThumb>(th => th.Add(c => c.Index, 0))
                    .AddChildContent<SliderThumb>(th => th.Add(c => c.Index, 1))));
        });
    }

    // ── SliderRoot ────────────────────────────────────────────────────────────────

    [Fact]
    public void Root_RendersDiv()
    {
        var cut = Render<SliderRoot>();
        Assert.Equal("DIV", cut.Find("div").TagName);
    }

    [Fact]
    public void Root_HasGroupRole()
    {
        var cut = Render<SliderRoot>();
        Assert.Equal("group", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void Root_HorizontalOrientationByDefault()
    {
        var cut = Render<SliderRoot>();
        Assert.Equal("horizontal", cut.Find("div").GetAttribute("data-orientation"));
    }

    [Fact]
    public void Root_VerticalOrientation_SetsDataAttribute()
    {
        var cut = Render<SliderRoot>(p => p.Add(c => c.Orientation, Orientation.Vertical));
        Assert.Equal("vertical", cut.Find("div").GetAttribute("data-orientation"));
    }

    [Fact]
    public void Root_Disabled_SetsDataAttribute()
    {
        var cut = Render<SliderRoot>(p => p.Add(c => c.Disabled, true));
        Assert.NotNull(cut.Find("[data-disabled]"));
    }

    [Fact]
    public void Root_NotDisabled_OmitsDataDisabled()
    {
        var cut = Render<SliderRoot>();
        Assert.Empty(cut.FindAll("[data-disabled]"));
    }

    [Fact]
    public void Root_HasAriaLabelledBy_PointingToDefaultLabelId()
    {
        // The root should have aria-labelledby pointing to its derived label id
        // even before a SliderLabel registers.
        var cut = Render<SliderRoot>();
        var div = cut.Find("div");
        var labelledBy = div.GetAttribute("aria-labelledby");
        Assert.NotNull(labelledBy);
        Assert.Contains("-label", labelledBy);
    }

    // ── SliderLabel ───────────────────────────────────────────────────────────────

    [Fact]
    public void Label_RendersDiv()
    {
        var cut = Render<SliderRoot>(p => p
            .AddChildContent<SliderLabel>(lp => lp.AddChildContent("Volume")));
        Assert.Equal("DIV", cut.Find("div > div").TagName);
    }

    [Fact]
    public void Label_IdMatchesRootAriaLabelledBy()
    {
        var cut = Render<SliderRoot>(p => p
            .AddChildContent<SliderLabel>(lp => lp.AddChildContent("Volume")));

        var rootDiv = cut.Find("div");
        var labelDiv = cut.Find("div > div");

        var ariaLabelledBy = rootDiv.GetAttribute("aria-labelledby");
        var labelId = labelDiv.GetAttribute("id");

        Assert.Equal(ariaLabelledBy, labelId);
    }

    [Fact]
    public void Label_SetsOrientationDataAttribute()
    {
        var cut = Render<SliderRoot>(p => p
            .Add(c => c.Orientation, Orientation.Vertical)
            .AddChildContent<SliderLabel>(lp => lp.AddChildContent("Volume")));

        Assert.Equal("vertical", cut.Find("div > div").GetAttribute("data-orientation"));
    }

    // ── SliderValue ───────────────────────────────────────────────────────────────

    [Fact]
    public void Value_RendersOutput()
    {
        var cut = Render<SliderRoot>(p => p
            .AddChildContent<SliderValue>());
        Assert.Equal("OUTPUT", cut.Find("output").TagName);
    }

    [Fact]
    public void Value_HasAriaLiveOff()
    {
        // aria-live="off" prevents screen readers from announcing every drag step.
        var cut = Render<SliderRoot>(p => p
            .AddChildContent<SliderValue>());
        Assert.Equal("off", cut.Find("output").GetAttribute("aria-live"));
    }

    [Fact]
    public void Value_DisplaysDefaultValue()
    {
        var cut = Render<SliderRoot>(p => p
            .Add(c => c.DefaultValue, 42.0)
            .AddChildContent<SliderValue>());
        Assert.Contains("42", cut.Find("output").TextContent);
    }

    [Fact]
    public void Value_DisplaysBothRangeValues()
    {
        var cut = Render<SliderRoot>(p => p
            .Add(c => c.DefaultValues, new[] { 20.0, 80.0 })
            .AddChildContent<SliderValue>());
        var text = cut.Find("output").TextContent;
        Assert.Contains("20", text);
        Assert.Contains("80", text);
    }

    [Fact]
    public void Value_HtmlForLinksToThumbInputs()
    {
        // The output's for attribute must reference the range inputs inside the thumbs.
        var cut = Render<SliderRoot>(p => p
            .Add(c => c.DefaultValue, 50.0)
            .AddChildContent(b =>
            {
                b.OpenComponent<SliderValue>(0);
                b.CloseComponent();
                b.OpenComponent<SliderControl>(1);
                b.AddAttribute(2, "ChildContent", (RenderFragment)(b2 =>
                {
                    b2.OpenComponent<SliderTrack>(3);
                    b2.AddAttribute(4, "ChildContent", (RenderFragment)(b3 =>
                    {
                        b3.OpenComponent<SliderThumb>(5);
                        b3.CloseComponent();
                    }));
                    b2.CloseComponent();
                }));
                b.CloseComponent();
            }));

        var output = cut.Find("output");
        var htmlFor = output.GetAttribute("for");
        Assert.NotNull(htmlFor);
        Assert.NotEmpty(htmlFor);

        // The for attribute may be a space-separated list of ids; the first id must
        // reference an actual range input on the page.
        var firstId = htmlFor.Split(' ')[0];
        var input = cut.Find($"input[id='{firstId}']");
        Assert.NotNull(input);
    }

    // ── SliderControl ─────────────────────────────────────────────────────────────

    [Fact]
    public void Control_RendersDiv()
    {
        var cut = RenderSlider();
        // The control is the direct child of root.
        Assert.NotNull(cut.Find("[data-orientation]"));
    }

    [Fact]
    public void Control_HasTabIndexMinusOne()
    {
        var cut = RenderSlider();
        // The control is focusable for pointer interactions but not in tab order.
        var controls = cut.FindAll("[tabindex='-1']");
        Assert.NotEmpty(controls);
    }

    [Fact]
    public void Control_HasOrientationDataAttribute()
    {
        var cut = RenderSlider(p => p.Add(c => c.Orientation, Orientation.Vertical));
        // All slider sub-parts with data-orientation should show vertical.
        var attrs = cut.FindAll("[data-orientation='vertical']");
        Assert.NotEmpty(attrs);
    }

    // ── SliderThumb ───────────────────────────────────────────────────────────────

    [Fact]
    public void Thumb_ContainsRangeInput()
    {
        // Base UI: the implicit role="slider" comes from the hidden input[type=range].
        var cut = RenderSlider(p => p.Add(c => c.DefaultValue, 30.0));
        var input = cut.Find("input[type='range']");
        Assert.NotNull(input);
    }

    [Fact]
    public void Thumb_InputHasAriaValueNow()
    {
        var cut = RenderSlider(p => p.Add(c => c.DefaultValue, 30.0));
        Assert.Equal("30", cut.Find("input[type='range']").GetAttribute("aria-valuenow"));
    }

    [Fact]
    public void Thumb_InputHasAriaOrientation()
    {
        var cut = RenderSlider();
        Assert.Equal("horizontal", cut.Find("input[type='range']").GetAttribute("aria-orientation"));
    }

    [Fact]
    public void Thumb_InputHasMinMaxStep()
    {
        var cut = RenderSlider(p => p
            .Add(c => c.Min, 10.0)
            .Add(c => c.Max, 90.0)
            .Add(c => c.Step, 5.0));
        var input = cut.Find("input[type='range']");
        Assert.Equal("10", input.GetAttribute("min"));
        Assert.Equal("90", input.GetAttribute("max"));
        Assert.Equal("5", input.GetAttribute("step"));
    }

    [Fact]
    public void Thumb_HasDataIndexAttribute()
    {
        var cut = RenderSlider();
        Assert.Equal("0", cut.Find("[data-index]").GetAttribute("data-index"));
    }

    [Fact]
    public void Thumb_Disabled_SetsDisabledOnInput()
    {
        var cut = RenderSlider(p => p.Add(c => c.Disabled, true));
        Assert.NotNull(cut.Find("input[disabled]"));
    }

    [Fact]
    public void Thumb_InputHasAriaLabelledBy()
    {
        var cut = Render<SliderRoot>(p => p
            .AddChildContent<SliderLabel>(lp => lp.AddChildContent("Volume"))
            .AddChildContent<SliderControl>(cp => cp
                .AddChildContent<SliderTrack>(tp => tp
                    .AddChildContent<SliderThumb>())));

        var input = cut.Find("input[type='range']");
        var labelledBy = input.GetAttribute("aria-labelledby");
        Assert.NotNull(labelledBy);

        // The input's aria-labelledby must reference the label element.
        var labelEl = cut.Find($"[id='{labelledBy}']");
        Assert.NotNull(labelEl);
    }

    // ── Range slider ──────────────────────────────────────────────────────────────

    [Fact]
    public void RangeSlider_TwoInputs()
    {
        var cut = RenderRangeSlider(p => p.Add(c => c.DefaultValues, new[] { 20.0, 80.0 }));
        Assert.Equal(2, cut.FindAll("input[type='range']").Count);
    }

    [Fact]
    public void RangeSlider_TwoThumbsHaveDistinctDataIndex()
    {
        var cut = RenderRangeSlider(p => p.Add(c => c.DefaultValues, new[] { 20.0, 80.0 }));
        var thumbs = cut.FindAll("[data-index]");
        Assert.Equal(2, thumbs.Count);
        Assert.Equal("0", thumbs[0].GetAttribute("data-index"));
        Assert.Equal("1", thumbs[1].GetAttribute("data-index"));
    }

    [Fact]
    public void RangeSlider_AriaValueText_StartRange_EndRange()
    {
        // Base UI sets "XX start range" / "XX end range" aria-valuetext for two-thumb sliders.
        var cut = RenderRangeSlider(p => p.Add(c => c.DefaultValues, new[] { 44.0, 50.0 }));
        var inputs = cut.FindAll("input[type='range']");
        Assert.Equal(2, inputs.Count);

        Assert.Contains("start range", inputs[0].GetAttribute("aria-valuetext") ?? "");
        Assert.Contains("end range", inputs[1].GetAttribute("aria-valuetext") ?? "");
    }

    [Fact]
    public void RangeSlider_SingleThumb_NoAriaValueText()
    {
        // Single-value sliders don't need aria-valuetext — aria-valuenow is sufficient.
        var cut = RenderSlider(p => p.Add(c => c.DefaultValue, 50.0));
        var input = cut.Find("input[type='range']");
        Assert.Null(input.GetAttribute("aria-valuetext"));
    }

    // ── SliderTrack ───────────────────────────────────────────────────────────────

    [Fact]
    public void Track_HasPositionRelativeStyle()
    {
        // The Track must set position:relative so that absolutely-positioned indicator
        // and thumb children are contained within the track bounds.
        var cut = RenderSlider();
        // The track is a descendant of the control; find it via its content structure.
        // The track's direct parent is the control (which has tabindex=-1).
        var allDivs = cut.FindAll("div");
        var track = allDivs.FirstOrDefault(d =>
        {
            var style = d.GetAttribute("style") ?? "";
            return style.Contains("position: relative");
        });
        Assert.NotNull(track);
    }

    // ── Data attribute completeness ───────────────────────────────────────────────

    [Fact]
    public void AllSubParts_HaveOrientationDataAttribute()
    {
        // Control, Track, Indicator, and Thumb all carry data-orientation.
        var cut = RenderSlider();
        var elems = cut.FindAll("[data-orientation]");
        // Expect at minimum: root, control, track, indicator, thumb.
        Assert.True(elems.Count >= 5, $"Expected at least 5 elements with data-orientation, got {elems.Count}");
    }

    [Fact]
    public void AllSubParts_DisabledDataAttribute_PropagatesDown()
    {
        var cut = RenderSlider(p => p.Add(c => c.Disabled, true));
        var disabled = cut.FindAll("[data-disabled]");
        // Root, control, track, indicator, and thumb should all carry data-disabled.
        Assert.True(disabled.Count >= 5, $"Expected at least 5 [data-disabled] elements, got {disabled.Count}");
    }
}
