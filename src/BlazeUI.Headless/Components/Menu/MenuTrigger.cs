using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Menu;

/// <summary>
/// A button that opens the menu.
/// Renders a <c>&lt;button&gt;</c> element.
/// </summary>
public class MenuTrigger : BlazeElement<MenuTriggerState>
{
    [CascadingParameter] internal MenuContext Context { get; set; } = default!;

    /// <summary>Whether the trigger should ignore user interaction.</summary>
    [Parameter] public bool Disabled { get; set; }

    protected override string DefaultTag => "button";
    protected override string ElementId => Id ?? Context.TriggerId;

    protected override void OnParametersSet()
    {
        // Propagate consumer Id into context so popup positioning and
        // aria-controls references stay consistent.
        if (Id is not null)
            Context.TriggerId = Id;
    }
    protected override MenuTriggerState GetCurrentState() => new(Context.Open, Disabled);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        // data-popup-open and data-pressed are both emitted when the menu is open,
        // matching Base UI's pressableTriggerOpenStateMapping.
        yield return new("data-popup-open", Context.Open ? "" : null);
        yield return new("data-pressed", Context.Open ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("aria-haspopup", "menu");
        yield return new("aria-expanded", Context.Open ? "true" : "false");
        yield return new("aria-controls", Context.PopupId);

        // A disabled trigger must not be interactive.
        if (Disabled)
        {
            yield return new("disabled", "");
        }
        else
        {
            yield return new("onclick",
                EventCallback.Factory.Create<MouseEventArgs>(this, () => Context.SetOpen(!Context.Open)));
        }
    }
}

public readonly record struct MenuTriggerState(bool Open, bool Disabled);
