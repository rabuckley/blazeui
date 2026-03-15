using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Progress;

/// <summary>
/// Visualizes the completion status of the task. Renders a <c>&lt;div&gt;</c> element.
/// </summary>
/// <remarks>
/// When a value is present, this element receives <c>inset-inline-start: 0</c>,
/// <c>height: inherit</c>, and <c>width: {percentage}%</c> as inline styles,
/// matching Base UI's indicator behaviour. No inline style is set when indeterminate.
/// </remarks>
public class ProgressIndicator : BlazeElement<ProgressRootState>
{
    [CascadingParameter]
    internal ProgressContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";

    protected override ProgressRootState GetCurrentState() => new(Context.Status);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-indeterminate", Context.Status is ProgressStatus.Indeterminate ? "" : null);
        yield return new("data-progressing", Context.Status is ProgressStatus.Progressing ? "" : null);
        yield return new("data-complete", Context.Status is ProgressStatus.Complete ? "" : null);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var attrs = BuildAttributes(state);
        var tag = As ?? DefaultTag;

        // Emit position/size styles only when a concrete value is known.
        // Merge after BuildAttributes so the indicator styles can be further
        // overridden by the consumer's Style parameter.
        if (Context.Percentage.HasValue)
        {
            var indicatorStyle = $"inset-inline-start: 0; height: inherit; width: {Context.Percentage.Value:0.##}%";
            if (attrs.TryGetValue("style", out var existing))
                attrs["style"] = Css.Cn(indicatorStyle, existing?.ToString());
            else
                attrs["style"] = indicatorStyle;
        }

        builder.OpenElement(0, tag);
        builder.AddMultipleAttributes(1, attrs);
        builder.AddContent(2, ChildContent);
        builder.CloseElement();
    }
}
