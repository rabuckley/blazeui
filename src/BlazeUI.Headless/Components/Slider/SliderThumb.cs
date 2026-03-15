using System.Globalization;
using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Slider;

/// <summary>
/// The draggable part of the slider at the tip of the indicator.
/// Renders a <c>&lt;div&gt;</c> element containing a hidden <c>&lt;input type="range"&gt;</c>.
/// </summary>
/// <remarks>
/// The hidden range input carries ARIA attributes (<c>role="slider"</c> is implicit on
/// <c>input[type=range]</c>) and is the focusable element. The outer div is non-focusable
/// and used for visual positioning only, matching Base UI's approach.
/// </remarks>
public class SliderThumb : BlazeElement<SliderThumbState>
{
    [CascadingParameter]
    internal SliderContext Context { get; set; } = default!;

    /// <summary>
    /// The zero-based index of this thumb in a range slider. Required when multiple
    /// thumbs are present. Defaults to 0 for single-value sliders.
    /// </summary>
    [Parameter] public int Index { get; set; }

    protected override string DefaultTag => "div";

    protected override SliderThumbState GetCurrentState() =>
        new(Context.Values, Index, Context.Orientation, Context.Disabled, Context.Dragging);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-index", Index.ToString(CultureInfo.InvariantCulture));
        yield return new("data-orientation", Context.Orientation == Orientation.Vertical ? "vertical" : "horizontal");
        yield return new("data-disabled", Context.Disabled ? "" : null);
        yield return new("data-dragging", Context.Dragging ? "" : null);
    }

    // The thumb value is the value at this index, clamped to the valid range.
    private double ThumbValue => Context.Values.Length > Index
        ? Context.Values[Index]
        : Context.Min;

    private double ThumbPercent => Context.Max > Context.Min
        ? (ThumbValue - Context.Min) / (Context.Max - Context.Min)
        : 0;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // The input id follows the same convention pre-populated in SliderRoot.OnParametersSet
        // so SliderValue's htmlFor is correct even when it renders before this thumb.
        var inputId = Context.ControlId + "-thumb-" + Index;

        var state = GetCurrentState();
        var percent = (ThumbPercent * 100).ToString(CultureInfo.InvariantCulture);

        // Base UI positions the thumb absolutely within the track. The edge-alignment
        // formula ensures the thumb's edge stays within the control bounds. Before JS
        // measures the thumb, --slider-thumb-size defaults to 0px and there is no edge
        // offset. Once JS reports the measured size, C# includes it in the inline style
        // so it survives Blazor re-renders (JS-set inline properties are wiped by diffs).
        // {percent} must be dimensionless (not {percent}%) so CSS calc can multiply
        // it with the (100% - thumbSize) length-percentage expression.
        var thumbSizeDecl = Context.ThumbSize > 0
            ? $"--slider-thumb-size: {Context.ThumbSize.ToString(CultureInfo.InvariantCulture)}px; "
            : "";
        var edgeCalc = $"calc(var(--slider-thumb-size, 0px) / 2 + {percent} / 100 * (100% - var(--slider-thumb-size, 0px)))";
        var positionStyle = Context.Orientation == Orientation.Vertical
            ? $"{thumbSizeDecl}position: absolute; inset-block-end: {edgeCalc}; left: 50%; translate: -50% 50%;"
            : $"{thumbSizeDecl}position: absolute; inset-inline-start: {edgeCalc}; top: 50%; translate: -50% -50%;";

        var attrs = BuildAttributes(state);

        // Merge in position style, preserving any consumer-supplied style.
        if (attrs.TryGetValue("style", out var existingStyle))
            attrs["style"] = Css.Cn(positionStyle, existingStyle?.ToString());
        else
            attrs["style"] = positionStyle;

        // The outer div is non-focusable; focus lives on the inner input.
        attrs["tabindex"] = "-1";

        var tag = As ?? DefaultTag;
        builder.OpenElement(0, tag);
        builder.AddMultipleAttributes(1, attrs);

        builder.AddContent(2, ChildContent);

        // Hidden range input: provides ARIA role="slider" semantics, keyboard interaction,
        // and form integration. Styled to be visually hidden but sized to 100%×100% of the
        // thumb so VoiceOver's focus indicator matches the thumb's visual bounds.
        builder.OpenElement(3, "input");
        builder.AddAttribute(4, "id", inputId);
        builder.AddAttribute(5, "type", "range");
        builder.AddAttribute(6, "aria-orientation", Context.Orientation == Orientation.Vertical ? "vertical" : "horizontal");
        builder.AddAttribute(7, "aria-valuenow", ThumbValue.ToString(CultureInfo.InvariantCulture));
        builder.AddAttribute(8, "aria-valuemin", Context.Min.ToString(CultureInfo.InvariantCulture));
        builder.AddAttribute(9, "aria-valuemax", Context.Max.ToString(CultureInfo.InvariantCulture));

        // For range sliders, provide descriptive aria-valuetext so screen readers
        // can distinguish which end of the range the thumb represents.
        var valueText = GetAriaValueText();
        if (valueText is not null)
            builder.AddAttribute(10, "aria-valuetext", valueText);

        // Associate the input with the slider label via aria-labelledby.
        var labelId = Context.LabelId ?? Context.RootLabelId;
        if (!string.IsNullOrEmpty(labelId))
            builder.AddAttribute(11, "aria-labelledby", labelId);

        builder.AddAttribute(12, "min", Context.Min.ToString(CultureInfo.InvariantCulture));
        builder.AddAttribute(13, "max", Context.Max.ToString(CultureInfo.InvariantCulture));
        builder.AddAttribute(14, "step", Context.Step.ToString(CultureInfo.InvariantCulture));
        builder.AddAttribute(15, "value", ThumbValue.ToString(CultureInfo.InvariantCulture));
        builder.AddAttribute(16, "disabled", Context.Disabled ? true : (object?)null);

        // Visually hidden, full-size so VoiceOver focus ring matches thumb bounds.
        builder.AddAttribute(17, "style",
            "clip-path: inset(50%); overflow: hidden; white-space: nowrap; border: 0; padding: 0; " +
            "width: 100%; height: 100%; margin: -1px; position: fixed; top: 0; left: 0;");

        builder.CloseElement(); // input

        builder.CloseElement(); // tag
    }

    /// <summary>
    /// Returns the <c>aria-valuetext</c> string for range sliders, matching Base UI's
    /// <c>getDefaultAriaValueText</c> helper. For single-value sliders returns <c>null</c>
    /// (the numeric <c>aria-valuenow</c> is sufficient).
    /// </summary>
    private string? GetAriaValueText()
    {
        if (!Context.IsRange) return null;

        var vals = Context.Values;
        if (vals.Length == 2)
        {
            return Index == 0
                ? $"{ThumbValue.ToString(CultureInfo.InvariantCulture)} start range"
                : $"{ThumbValue.ToString(CultureInfo.InvariantCulture)} end range";
        }

        // For 3+ thumbs just return the value as a string so screen readers have something.
        return ThumbValue.ToString(CultureInfo.InvariantCulture);
    }
}

public readonly record struct SliderThumbState(
    double[] Values,
    int Index,
    Orientation Orientation,
    bool Disabled,
    bool Dragging);
