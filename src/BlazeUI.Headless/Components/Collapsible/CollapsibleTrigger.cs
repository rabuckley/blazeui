using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Collapsible;

public class CollapsibleTrigger : BlazeElement<CollapsibleTriggerState>
{
    [CascadingParameter] internal CollapsibleContext Context { get; set; } = default!;

    protected override string DefaultTag => "button";
    protected override string ElementId => Id ?? Context.TriggerId;

    protected override void OnParametersSet()
    {
        // Propagate consumer Id into context so CollapsiblePanel's aria-controls
        // and related cross-references stay consistent.
        if (Id is not null)
            Context.TriggerId = Id;
    }
    protected override CollapsibleTriggerState GetCurrentState() => new(Context.Open, Context.Disabled);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-panel-open", Context.Open ? "" : null);
        yield return new("data-disabled", Context.Disabled ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("aria-expanded", Context.Open ? "true" : "false");

        // aria-controls references the panel only when it is open and therefore
        // rendered; it is omitted when closed so the trigger doesn't point to a
        // non-existent element, matching Base UI's behaviour.
        if (Context.Open)
            yield return new("aria-controls", Context.PanelId);

        // Base UI uses focusableWhenDisabled: true, so disabled triggers remain focusable
        // and disabled state is communicated via aria-disabled rather than the native attribute.
        if (Context.Disabled)
            yield return new("aria-disabled", "true");

        yield return new("onclick",
            EventCallback.Factory.Create<MouseEventArgs>(this, () => Context.Toggle()));
    }
}

public readonly record struct CollapsibleTriggerState(bool Open, bool Disabled);
