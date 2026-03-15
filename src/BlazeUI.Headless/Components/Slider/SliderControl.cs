using System.Globalization;
using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Slider;

/// <summary>
/// The clickable, interactive part of the slider.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
public class SliderControl : BlazeElement<SliderControlState>
{
    [CascadingParameter]
    internal SliderContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";

    protected override SliderControlState GetCurrentState() =>
        new(Context.Orientation, Context.Disabled, Context.Dragging);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-orientation", Context.Orientation == Orientation.Vertical ? "vertical" : "horizontal");
        yield return new("data-disabled", Context.Disabled ? "" : null);
        yield return new("data-dragging", Context.Dragging ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        // tabindex="-1" allows the control to receive programmatic focus for pointer
        // interactions without being in the normal tab order (thumbs are the tab stops).
        yield return new("tabindex", "-1");
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var attrs = BuildAttributes(state);

        // Expose the first thumb's percentage as a CSS custom property for styling hooks.
        // Appended to existing style so consumer styles are not overridden.
        var percent = Context.Values.Length > 0
            ? ((Context.Values[0] - Context.Min) / Math.Max(Context.Max - Context.Min, 1) * 100).ToString(CultureInfo.InvariantCulture)
            : "0";
        var percentVar = $"--slider-percent: {percent}";
        if (attrs.TryGetValue("style", out var existingStyle))
            attrs["style"] = Css.Cn(percentVar, existingStyle?.ToString());
        else
            attrs["style"] = percentVar;

        // Assign the context-provided id so JS can look up this element by id.
        attrs["id"] = Context.ControlId;

        var tag = As ?? DefaultTag;
        builder.OpenElement(0, tag);
        builder.AddMultipleAttributes(1, attrs);
        builder.AddContent(2, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct SliderControlState(Orientation Orientation, bool Disabled, bool Dragging);
