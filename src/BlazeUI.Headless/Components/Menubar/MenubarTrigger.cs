using BlazeUI.Headless.Components.Menu;
using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Menubar;

/// <summary>
/// Trigger button for a menubar menu. Participates in the menubar's roving tabindex
/// and delegates open/close to the parent <see cref="MenubarMenu"/>.
/// </summary>
public class MenubarTrigger : BlazeElement<MenubarTriggerState>
{
    [CascadingParameter] internal MenubarContext MenubarContext { get; set; } = default!;
    [CascadingParameter] internal MenuContext MenuContext { get; set; } = default!;

    protected override string DefaultTag => "button";
    protected override string ElementId => Id ?? MenuContext.TriggerId;

    protected override void OnParametersSet()
    {
        // Propagate consumer Id into context so popup positioning stays consistent.
        if (Id is not null)
            MenuContext.TriggerId = Id;
    }

    protected override MenubarTriggerState GetCurrentState() =>
        new(MenuContext.Open, MenubarContext.Disabled);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        // data-popup-open tracks whether this trigger's menu is open.
        yield return new("data-popup-open", MenuContext.Open ? "" : null);
        // data-pressed mirrors data-popup-open: Base UI uses pressableTriggerOpenStateMapping
        // which emits both attributes when the menu is open.
        yield return new("data-pressed", MenuContext.Open ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("role", "menuitem");
        yield return new("aria-haspopup", "menu");
        yield return new("aria-expanded", MenuContext.Open ? "true" : "false");
        yield return new("tabindex", "0");
        if (MenubarContext.Disabled)
        {
            yield return new("disabled", "");
            yield return new("aria-disabled", "true");
        }
        else
        {
            yield return new("onclick",
                EventCallback.Factory.Create<MouseEventArgs>(this, () => MenuContext.SetOpen(!MenuContext.Open)));

            // When another menu in the menubar is already open, hovering this trigger
            // should switch to this menu — standard menubar UX behavior.
            yield return new("onpointerenter",
                EventCallback.Factory.Create<PointerEventArgs>(this, () =>
                {
                    if (MenubarContext.HasSubmenuOpen && !MenuContext.Open)
                        return MenuContext.SetOpen(true);
                    return Task.CompletedTask;
                }));
        }
    }
}

public readonly record struct MenubarTriggerState(bool Open, bool Disabled);
