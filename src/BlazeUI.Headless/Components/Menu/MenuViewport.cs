using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Menu;

/// <summary>
/// A viewport for displaying content transitions when one popup is opened by multiple detached
/// triggers and the content changes based on the active trigger.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
/// <remarks>
/// This component is only needed when a menu popup can be opened by multiple triggers (via
/// <c>handle</c>/<c>MenuHandle</c>) and transitions between their content should be animated.
/// For ordinary single-trigger menus this component has no effect.
/// </remarks>
// TODO: animated viewport transitions (activationDirection, transitioning CSS vars) require
// the same usePopupViewport logic as Base UI's React hook, which tracks previous content
// and injects CSS variables for the animation. Deferred until detached-trigger + viewport
// animation is needed in practice.
public class MenuViewport : BlazeElement<MenuViewportState>
{
    [CascadingParameter] internal MenuContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";

    // MenuViewport has no open/closed data attributes. Base UI's MenuViewportDataAttributes
    // only defines data-activation-direction, data-transitioning, data-current, data-previous,
    // and data-instant — all of which require the usePopupViewport hook logic (deferred).
    protected override MenuViewportState GetCurrentState() => default;
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes() { yield break; }
}

public readonly record struct MenuViewportState;
