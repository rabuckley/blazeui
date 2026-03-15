using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Tooltip;

public class TooltipTrigger : BlazeElement<TooltipTriggerState>
{
    [CascadingParameter]
    internal TooltipContext Context { get; set; } = default!;

    /// <summary>
    /// When true, this trigger will not open the tooltip on hover or focus.
    /// Falls back to the root's <see cref="TooltipRoot.Disabled"/> when not set.
    /// Does not apply the HTML <c>disabled</c> attribute to the trigger element.
    /// </summary>
    [Parameter] public bool? Disabled { get; set; }

    // Effective disabled state: per-trigger override takes precedence over root.
    private bool IsDisabled => Disabled ?? Context.Disabled;

    protected override string DefaultTag => "button";
    protected override string ElementId => Context.TriggerId;

    protected override TooltipTriggerState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-popup-open", Context.Open ? "" : null);
        // data-trigger-disabled is present whenever the trigger is effectively disabled,
        // whether via the per-trigger prop or the root Disabled flag.
        yield return new("data-trigger-disabled", IsDisabled ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        if (Context.Open)
            yield return new("aria-describedby", Context.PopupId);
    }
}

public readonly record struct TooltipTriggerState(bool Open);
