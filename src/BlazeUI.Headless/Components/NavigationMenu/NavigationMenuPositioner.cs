using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.NavigationMenu;

/// <summary>
/// Positioning wrapper for navigation menu popup content. In Base UI this
/// handles floating-ui positioning; BlazeUI's architecture uses a Viewport
/// approach instead, so this is a structural wrapper that emits positioning
/// data attributes (<c>data-side</c>, <c>data-align</c>) for CSS-driven layout.
/// </summary>
/// <remarks>
/// Place this inside the <see cref="NavigationMenuViewport"/> and wrap
/// <see cref="NavigationMenuPopup"/> or content directly. The component does
/// not perform JS-based positioning — it provides the semantic structure that
/// Base UI consumers expect.
/// </remarks>
public class NavigationMenuPositioner : BlazeElement<NavigationMenuPositionerState>
{
    [CascadingParameter] internal NavigationMenuContext Context { get; set; } = default!;

    [Parameter] public Side Side { get; set; } = Side.Bottom;
    [Parameter] public Align Align { get; set; } = Align.Center;

    protected override string DefaultTag => "div";

    private bool IsOpen => Context.ActiveValue is not null;

    protected override NavigationMenuPositionerState GetCurrentState() =>
        new(IsOpen, Side.ToString().ToLowerInvariant(), Align.ToString().ToLowerInvariant());

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-open", IsOpen ? "" : null);
        yield return new("data-closed", !IsOpen ? "" : null);
        yield return new("data-side", Side.ToString().ToLowerInvariant());
        yield return new("data-align", Align.ToString().ToLowerInvariant());
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("role", "presentation");
    }
}

public readonly record struct NavigationMenuPositionerState(bool Open, string Side, string Align);
