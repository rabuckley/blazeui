using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Progress;

/// <summary>
/// A text label displaying the current value. Renders a <c>&lt;span&gt;</c> element.
/// </summary>
/// <remarks>
/// Marked <c>aria-hidden="true"</c> — the progress value is already communicated to
/// assistive technology via the root's <c>aria-valuetext</c> attribute.
/// </remarks>
public class ProgressValue : BlazeElement<ProgressRootState>
{
    [CascadingParameter] internal ProgressContext Context { get; set; } = default!;

    protected override string DefaultTag => "span";

    protected override ProgressRootState GetCurrentState() => new(Context.Status);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-indeterminate", Context.Status is ProgressStatus.Indeterminate ? "" : null);
        yield return new("data-progressing", Context.Status is ProgressStatus.Progressing ? "" : null);
        yield return new("data-complete", Context.Status is ProgressStatus.Complete ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        // Hidden from assistive technology — the formatted value is already
        // communicated via aria-valuetext on the progressbar root.
        yield return new("aria-hidden", "true");
    }

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var attrs = BuildAttributes(state);
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddMultipleAttributes(1, attrs);

        // Render the formatted value, or nothing when indeterminate — the consumer's
        // ChildContent takes precedence if provided.
        if (ChildContent is not null)
        {
            builder.AddContent(2, ChildContent);
        }
        else
        {
            var display = Context.Status is ProgressStatus.Indeterminate
                ? null
                : Context.FormattedValue;
            builder.AddContent(2, display);
        }

        builder.CloseElement();
    }
}
