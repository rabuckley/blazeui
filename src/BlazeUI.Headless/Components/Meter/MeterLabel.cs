using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Meter;

/// <summary>
/// An accessible label for the meter. Renders a <c>&lt;span&gt;</c> element and
/// registers its ID with <see cref="MeterRoot"/> so the root can emit
/// <c>aria-labelledby</c> pointing to this element.
/// </summary>
public class MeterLabel : BlazeElement<MeterLabelState>
{
    [CascadingParameter] internal MeterContext Context { get; set; } = default!;

    private string _labelId = "";

    protected override string DefaultTag => "span";

    protected override void OnInitialized()
    {
        // Generate and register the label ID with the root. The root listens via
        // the onChanged callback on MeterContext and re-renders to apply aria-labelledby.
        _labelId = IdGenerator.Next("meter-label");
        Context.SetLabelId(_labelId);
    }

    // Use the registered label ID rather than the default ResolvedId so the root's
    // aria-labelledby value always matches this element's actual id attribute.
    // Consumer Id takes precedence for stable testing/ARIA IDs.
    protected override string ElementId => Id ?? _labelId;

    protected override void OnParametersSet()
    {
        // Propagate effective ID into context so MeterRoot's
        // aria-labelledby points at the right element. Guard against
        // redundant updates — SetLabelId triggers a Root re-render which
        // would re-cascade parameters and cause an infinite loop.
        var effectiveId = Id ?? _labelId;
        if (Context.LabelId != effectiveId)
            Context.SetLabelId(effectiveId);
    }

    protected override MeterLabelState GetCurrentState() => default;

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield break;
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        // Base UI renders MeterLabel with role="presentation" to prevent assistive
        // technologies from treating it as a landmark or interactive element — the
        // label's text is surfaced to AT exclusively via the root's aria-labelledby.
        yield return new("role", "presentation");
    }
}

public readonly record struct MeterLabelState;
