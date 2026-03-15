using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Meter;

/// <summary>
/// Visualizes the position of the value along the range.
/// Renders a <c>&lt;div&gt;</c> element with an inline style that sets
/// <c>inset-inline-start</c>, <c>height</c>, and <c>width</c> to represent
/// the current value as a proportion of the min–max range.
/// </summary>
public class MeterIndicator : BlazeElement<MeterState>
{
    [CascadingParameter]
    internal MeterContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";

    protected override MeterState GetCurrentState() => new(Context.Value, Context.Min, Context.Max);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield break;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddAttribute(1, "id", ResolvedId);

        if (AdditionalAttributes is not null)
            builder.AddMultipleAttributes(2, AdditionalAttributes);

        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass))
            builder.AddAttribute(3, "class", mergedClass);

        // Mirrors Base UI's inline style: inset-inline-start fixes the left edge,
        // height:inherit fills the track height, and width reflects the percentage.
        var percentageWidth = (Context.Value - Context.Min) / (Context.Max - Context.Min) * 100;
        var indicatorStyle = $"inset-inline-start: 0; height: inherit; width: {percentageWidth:0.##}%";
        var mergedStyle = Css.Cn(indicatorStyle, Style, StyleBuilder?.Invoke(state));
        builder.AddAttribute(4, "style", mergedStyle);

        builder.AddContent(5, ChildContent);
        builder.CloseElement();
    }
}
