using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.NavigationMenu;

/// <summary>
/// Chevron indicator for a navigation menu trigger. Rotates when the owning
/// item is active (open). Place inside a <see cref="NavigationMenuTrigger"/>.
/// </summary>
/// <remarks>
/// Renders a <c>&lt;span&gt;</c> with <c>aria-hidden="true"</c>. The default child
/// content is a downward chevron character (▼). Consumers can replace it with a
/// custom icon via <see cref="BlazeElement{TState}.ChildContent"/>.
/// </remarks>
public class NavigationMenuIcon : BlazeElement<NavigationMenuIconState>
{
    [CascadingParameter] internal NavigationMenuContext Context { get; set; } = default!;

    /// <summary>
    /// The value of the parent <see cref="NavigationMenuItem"/>. Used to determine
    /// whether this icon's owning item is currently active.
    /// </summary>
    [Parameter, EditorRequired] public string Value { get; set; } = "";

    protected override string DefaultTag => "span";

    private bool IsOpen => Context.ActiveValue == Value;

    protected override NavigationMenuIconState GetCurrentState() => new(IsOpen);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        // Base UI uses triggerOpenStateMapping (data-popup-open) rather than popupStateMapping
        // (data-open / data-closed) for the icon indicator.
        yield return new("data-popup-open", IsOpen ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("aria-hidden", "true");
    }
}

public readonly record struct NavigationMenuIconState(bool Open);
