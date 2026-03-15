using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Menu;

/// <summary>
/// Opens a submenu when clicked or hovered. Renders as a menuitem inside the parent menu.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
public class MenuSubmenuTrigger : BlazeElement<MenuSubmenuTriggerState>
{
    // Context from the submenu — used to open/close it.
    [CascadingParameter] internal MenuContext Context { get; set; } = default!;

    // The submenu trigger participates as an item in the parent menu's highlight
    // tracking. The parent menu cascades the same MenuContext, but the submenu trigger
    // identifies itself by the submenu's TriggerId.
    // NOTE: highlighted tracking depends on the JS layer reporting which item is active
    // via OnHighlightChange. The data-highlighted attribute is emitted here but will only
    // be set once JS is wired up.

    [Parameter] public bool Disabled { get; set; }

    protected override string DefaultTag => "div";
    protected override string ElementId => Id ?? Context.TriggerId;

    protected override void OnParametersSet()
    {
        // Propagate consumer Id into context so submenu positioning stays consistent.
        if (Id is not null)
            Context.TriggerId = Id;
    }
    protected override MenuSubmenuTriggerState GetCurrentState() => new(Context.Open, Disabled, IsHighlighted);

    private bool IsHighlighted => Context.HighlightedItemId == ResolvedId;

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        // data-popup-open matches Base UI's triggerOpenStateMapping (not pressable — no data-pressed).
        yield return new("data-popup-open", Context.Open ? "" : null);
        yield return new("data-highlighted", IsHighlighted ? "" : null);
        yield return new("data-disabled", Disabled ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("role", "menuitem");
        yield return new("aria-haspopup", "menu");
        yield return new("aria-expanded", Context.Open ? "true" : "false");
        yield return new("tabindex", Disabled ? "-1" : "0");
        if (!Disabled)
        {
            yield return new("onclick",
                EventCallback.Factory.Create<MouseEventArgs>(this, () => Context.SetOpen(!Context.Open)));

            // Hovering a submenu trigger opens its submenu — standard menu UX.
            yield return new("onpointerenter",
                EventCallback.Factory.Create<PointerEventArgs>(this, () =>
                {
                    if (!Context.Open)
                        return Context.SetOpen(true);
                    return Task.CompletedTask;
                }));
        }
    }
}

public readonly record struct MenuSubmenuTriggerState(bool Open, bool Disabled, bool Highlighted);
