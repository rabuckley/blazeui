using System.Globalization;
using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Slider;

/// <summary>
/// Displays the current value of the slider as text.
/// Renders an <c>&lt;output&gt;</c> element.
/// </summary>
public class SliderValue : BlazeElement<SliderValueState>
{
    [CascadingParameter]
    internal SliderContext Context { get; set; } = default!;

    protected override string DefaultTag => "output";

    protected override SliderValueState GetCurrentState() =>
        new(Context.Values, Context.Orientation, Context.Disabled, Context.Dragging);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-orientation", Context.Orientation == Orientation.Vertical ? "vertical" : "horizontal");
        yield return new("data-disabled", Context.Disabled ? "" : null);
        yield return new("data-dragging", Context.Dragging ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        // aria-live="off" matches Base UI default: keep off while dragging to avoid
        // screen readers announcing every intermediate value during a drag gesture.
        yield return new("aria-live", "off");

        // htmlFor links the output to the hidden range inputs inside each thumb,
        // matching the Base UI SliderValue outputFor computation.
        var htmlFor = string.Join(" ", Context.ThumbInputIds);
        if (!string.IsNullOrEmpty(htmlFor))
            yield return new("for", htmlFor);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var attrs = BuildAttributes(state);

        var tag = As ?? DefaultTag;
        builder.OpenElement(0, tag);
        builder.AddMultipleAttributes(1, attrs);

        if (ChildContent is not null)
            builder.AddContent(2, ChildContent);
        else
        {
            // Default display: join values with an en-dash for range sliders.
            var text = string.Join(" \u2013 ",
                Context.Values.Select(v => v.ToString(CultureInfo.InvariantCulture)));
            builder.AddContent(2, text);
        }

        builder.CloseElement();
    }
}

public readonly record struct SliderValueState(
    double[] Values,
    Orientation Orientation,
    bool Disabled,
    bool Dragging);
