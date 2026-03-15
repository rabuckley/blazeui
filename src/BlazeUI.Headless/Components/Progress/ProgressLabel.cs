using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Progress;

/// <summary>
/// An accessible label for the progress bar. Renders a <c>&lt;span&gt;</c> element.
/// </summary>
/// <remarks>
/// Registering a <c>ProgressLabel</c> inside a <c>ProgressRoot</c> automatically wires
/// <c>aria-labelledby</c> on the root's progressbar element.
/// </remarks>
public class ProgressLabel : BlazeElement<ProgressRootState>
{
    [CascadingParameter] internal ProgressContext Context { get; set; } = default!;

    private string _labelId = "";

    protected override string DefaultTag => "span";

    protected override void OnInitialized()
    {
        // Generate a stable ID once and register it with the Root so it can
        // emit aria-labelledby pointing at this label. SetLabelId triggers a
        // Root re-render so aria-labelledby is emitted on the next render cycle.
        _labelId = IdGenerator.Next("progress-label");
        Context.SetLabelId?.Invoke(_labelId);
    }

    // Use the registered label ID rather than the generic ResolvedId.
    // Consumer Id takes precedence for stable testing/ARIA IDs.
    protected override string ElementId => Id ?? _labelId;

    protected override void OnParametersSet()
    {
        // Propagate effective ID into context so ProgressRoot's
        // aria-labelledby points at the right element. Guard against
        // redundant updates — SetLabelId triggers a Root re-render which
        // would re-cascade parameters and cause an infinite loop.
        var effectiveId = Id ?? _labelId;
        if (Context.LabelId != effectiveId)
            Context.SetLabelId?.Invoke(effectiveId);
    }

    protected override ProgressRootState GetCurrentState() => new(Context.Status);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-indeterminate", Context.Status is ProgressStatus.Indeterminate ? "" : null);
        yield return new("data-progressing", Context.Status is ProgressStatus.Progressing ? "" : null);
        yield return new("data-complete", Context.Status is ProgressStatus.Complete ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        // role="presentation" matches Base UI — the label is for visual/AT purposes only,
        // not an interactive element.
        yield return new("role", "presentation");
    }
}
