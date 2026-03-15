using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Slider;

/// <summary>
/// An accessible label that is automatically associated with the slider thumbs.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
/// <remarks>
/// The label id is fixed to the root-derived id (<c>{rootId}-label</c>) so that the
/// root's <c>aria-labelledby</c> attribute can reference it without requiring a
/// post-render registration callback. Clicking the label focuses the first thumb input.
/// </remarks>
public class SliderLabel : BlazeElement<SliderLabelState>
{
    [CascadingParameter]
    internal SliderContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";

    protected override SliderLabelState GetCurrentState() => new(Context.Orientation, Context.Disabled);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-orientation", Context.Orientation == Orientation.Vertical ? "vertical" : "horizontal");
        yield return new("data-disabled", Context.Disabled ? "" : null);
        yield return new("data-dragging", Context.Dragging ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        // Use the root-derived label id so the root's aria-labelledby already points here
        // without a post-render registration step.
        yield return new("id", Context.RootLabelId);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var attrs = BuildAttributes(state);

        var tag = As ?? DefaultTag;
        builder.OpenElement(0, tag);
        builder.AddMultipleAttributes(1, attrs);
        builder.AddContent(2, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct SliderLabelState(Orientation Orientation, bool Disabled);
