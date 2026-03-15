using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Drawer;

/// <summary>
/// Scrollable content area within the drawer popup.
/// Emits <c>data-drawer-content</c> so that swipe gesture logic
/// (in <see cref="DrawerSwipeArea"/> and <see cref="DrawerViewport"/>) can identify
/// scrollable content and avoid treating scroll as a dismiss swipe.
/// </summary>
public class DrawerContent : BlazeElement<DrawerContentState>
{
    [CascadingParameter] internal DrawerContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";

    protected override DrawerContentState GetCurrentState() => default;

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        // data-drawer-content is the only attribute Base UI emits on this element.
        // It acts as a selector hook for the swipe-dismiss logic to identify
        // scrollable regions that should not compete with drawer dismiss gestures.
        yield return new("data-drawer-content", "");
    }
}

public readonly record struct DrawerContentState;
