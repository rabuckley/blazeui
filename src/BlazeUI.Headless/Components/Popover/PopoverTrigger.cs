using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Popover;

/// <summary>
/// A button that opens the popover. Renders a <c>&lt;button&gt;</c> element.
/// </summary>
public class PopoverTrigger : BlazeElement<PopoverTriggerState>
{
    [CascadingParameter]
    internal PopoverContext Context { get; set; } = default!;

    protected override string DefaultTag => "button";
    protected override string ElementId => Id ?? Context.TriggerId;

    protected override void OnParametersSet()
    {
        // Propagate consumer Id into context so popup positioning and
        // aria-controls references stay consistent.
        if (Id is not null)
            Context.TriggerId = Id;
    }

    protected override PopoverTriggerState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        // data-popup-open: present when the associated popover is open.
        yield return new("data-popup-open", Context.Open ? "" : null);
    }

    private async Task HandleClick()
    {
        await Context.SetOpen(!Context.Open);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        // aria-expanded signals whether the controlled dialog popup is open.
        yield return new("aria-expanded", Context.Open ? "true" : "false");
        // aria-controls identifies the popup element this trigger controls.
        yield return new("aria-controls", Context.PopupId);
        yield return new("onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));
    }
}

public readonly record struct PopoverTriggerState(bool Open);
