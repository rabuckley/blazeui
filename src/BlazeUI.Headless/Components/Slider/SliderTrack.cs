using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Slider;

/// <summary>
/// Contains the slider indicator and represents the entire range of the slider.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
public class SliderTrack : BlazeElement<SliderTrackState>
{
    [CascadingParameter]
    internal SliderContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";

    protected override SliderTrackState GetCurrentState() =>
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

        // Base UI sets position: relative on the track so that the absolutely-positioned
        // indicator and thumbs are positioned relative to the track bounds.
        if (attrs.TryGetValue("style", out var existingStyle))
            attrs["style"] = Css.Cn("position: relative;", existingStyle?.ToString());
        else
            attrs["style"] = "position: relative;";

        var tag = As ?? DefaultTag;
        builder.OpenElement(0, tag);
        builder.AddMultipleAttributes(1, attrs);
        builder.AddContent(2, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct SliderTrackState(Orientation Orientation, bool Disabled, bool Dragging);
