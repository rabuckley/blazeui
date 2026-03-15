using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Progress;

/// <summary>
/// Contains the progress bar indicator. Renders a <c>&lt;div&gt;</c> element.
/// </summary>
public class ProgressTrack : BlazeElement<ProgressRootState>
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
}
