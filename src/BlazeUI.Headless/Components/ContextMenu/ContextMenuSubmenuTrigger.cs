using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.ContextMenu;

/// <summary>
/// Opens context menu submenu on hover or arrow-right. Renders as a menuitem.
/// </summary>
public class ContextMenuSubmenuTrigger : BlazeElement<ContextMenuSubmenuTriggerState>
{
    [CascadingParameter] internal ContextMenuContext Context { get; set; } = default!;

    /// <summary>Whether the component should ignore user interaction.</summary>
    [Parameter] public bool Disabled { get; set; }

    private bool IsHighlighted => Context.HighlightedItemId == ResolvedId;

    protected override string DefaultTag => "div";
    protected override ContextMenuSubmenuTriggerState GetCurrentState() => new(Context.Open, Disabled, IsHighlighted);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
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
        if (Disabled)
            yield return new("aria-disabled", "true");
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

public readonly record struct ContextMenuSubmenuTriggerState(bool Open, bool Disabled, bool Highlighted);
