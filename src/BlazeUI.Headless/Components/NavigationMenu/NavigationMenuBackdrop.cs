using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.NavigationMenu;

/// <summary>
/// Semi-transparent backdrop overlay for the navigation menu. Visible when any
/// menu item is active. Typically used to dim the page behind an open dropdown.
/// </summary>
public class NavigationMenuBackdrop : BlazeElement<NavigationMenuBackdropState>
{
    [CascadingParameter] internal NavigationMenuContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";

    private bool IsOpen => Context.ActiveValue is not null;

    protected override NavigationMenuBackdropState GetCurrentState() => new(IsOpen);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-open", IsOpen ? "" : null);
        yield return new("data-closed", !IsOpen ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("role", "presentation");
        if (!IsOpen)
            yield return new("hidden", true);
    }
}

public readonly record struct NavigationMenuBackdropState(bool Open);
