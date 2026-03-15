using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.NavigationMenu;

/// <summary>
/// Container for the navigation menu dropdown content. In Base UI this lives
/// inside a Positioner; in BlazeUI's architecture the Viewport renders content
/// directly, so this component is a thin wrapper that provides the <c>data-open</c>/
/// <c>data-closed</c> and <c>data-side</c> attributes for styling.
/// </summary>
/// <remarks>
/// Use this inside a <see cref="NavigationMenuViewport"/> or
/// <see cref="NavigationMenuPositioner"/> to wrap content with semantic markup
/// and transition-friendly data attributes.
/// </remarks>
public class NavigationMenuPopup : BlazeElement<NavigationMenuPopupState>
{
    [CascadingParameter] internal NavigationMenuContext Context { get; set; } = default!;

    protected override string DefaultTag => "nav";

    private bool IsOpen => Context.ActiveValue is not null;

    protected override NavigationMenuPopupState GetCurrentState() => new(IsOpen);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-open", IsOpen ? "" : null);
        yield return new("data-closed", !IsOpen ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("tabindex", "-1");
    }
}

public readonly record struct NavigationMenuPopupState(bool Open);
