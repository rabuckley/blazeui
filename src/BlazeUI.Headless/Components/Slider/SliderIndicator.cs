using System.Globalization;
using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Slider;

/// <summary>
/// Visualizes the current value of the slider.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
public class SliderIndicator : BlazeElement<SliderIndicatorState>
{
    [CascadingParameter]
    internal SliderContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";

    protected override SliderIndicatorState GetCurrentState() =>
        new(Context.Orientation, Context.Disabled, Context.Dragging);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-orientation", Context.Orientation == Orientation.Vertical ? "vertical" : "horizontal");
        yield return new("data-disabled", Context.Disabled ? "" : null);
        yield return new("data-dragging", Context.Dragging ? "" : null);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var attrs = BuildAttributes(state);

        var indicatorStyle = BuildIndicatorStyle();

        if (attrs.TryGetValue("style", out var existingStyle))
            attrs["style"] = Css.Cn(indicatorStyle, existingStyle?.ToString());
        else
            attrs["style"] = indicatorStyle;

        var tag = As ?? DefaultTag;
        builder.OpenElement(0, tag);
        builder.AddMultipleAttributes(1, attrs);
        builder.AddContent(2, ChildContent);
        builder.CloseElement();
    }

    /// <summary>
    /// Computes the inline style for the indicator based on the current value(s) and orientation,
    /// using edge-aligned positioning that matches the thumb's inset formula. The indicator
    /// extends from the track edge (or range start) to the thumb center, accounting for
    /// <c>--slider-thumb-size</c> so the indicator visually ends at the thumb midpoint.
    /// </summary>
    private string BuildIndicatorStyle()
    {
        var vals = Context.Values;
        if (vals.Length == 0) return "";

        var range = Context.Max - Context.Min;
        if (range <= 0) return "";

        var startPercent = (vals[0] - Context.Min) / range * 100;
        var endPercent = (vals[^1] - Context.Min) / range * 100;

        var start = startPercent.ToString(CultureInfo.InvariantCulture);
        var end = endPercent.ToString(CultureInfo.InvariantCulture);

        // When JS has measured the thumb size, use the same edge-compensation calc as
        // SliderThumb so the indicator aligns with the thumb center. Before measurement,
        // fall back to simple percentages (--slider-thumb-size defaults to 0px).
        // The formula: thumbSize/2 + percent/100 * (100% - thumbSize)
        static string EdgeCalc(string percent) =>
            $"calc(var(--slider-thumb-size, 0px) / 2 + {percent} / 100 * (100% - var(--slider-thumb-size, 0px)))";

        var thumbSizeDecl = Context.ThumbSize > 0
            ? $"--slider-thumb-size: {Context.ThumbSize.ToString(CultureInfo.InvariantCulture)}px; "
            : "";

        if (Context.Orientation == Orientation.Vertical)
        {
            if (Context.IsRange)
            {
                var startCalc = EdgeCalc(start);
                var endCalc = EdgeCalc(end);
                return $"{thumbSizeDecl}position: absolute; width: inherit; inset-block-end: {startCalc}; height: calc({endCalc} - {startCalc});";
            }
            var sizeCalc = EdgeCalc(start);
            return $"{thumbSizeDecl}position: absolute; width: inherit; inset-block-end: 0; height: {sizeCalc};";
        }
        else
        {
            if (Context.IsRange)
            {
                var startCalc = EdgeCalc(start);
                var endCalc = EdgeCalc(end);
                return $"{thumbSizeDecl}position: relative; height: inherit; inset-inline-start: {startCalc}; width: calc({endCalc} - {startCalc});";
            }
            var sizeCalc = EdgeCalc(start);
            return $"{thumbSizeDecl}position: relative; height: inherit; inset-inline-start: 0; width: {sizeCalc};";
        }
    }
}

public readonly record struct SliderIndicatorState(Orientation Orientation, bool Disabled, bool Dragging);
