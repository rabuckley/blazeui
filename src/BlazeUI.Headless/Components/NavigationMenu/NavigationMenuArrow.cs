using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.NavigationMenu;

/// <summary>
/// Arrow element for the navigation menu popup. Positioned via CSS to point
/// from the popup toward the active trigger.
/// </summary>
public class NavigationMenuArrow : BlazeElement<NavigationMenuArrowState>
{
    [CascadingParameter] internal NavigationMenuContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";

    private bool IsOpen => Context.ActiveValue is not null;

    protected override NavigationMenuArrowState GetCurrentState() => new(IsOpen);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-open", IsOpen ? "" : null);
        yield return new("data-closed", !IsOpen ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("aria-hidden", "true");
    }
}

public readonly record struct NavigationMenuArrowState(bool Open);
