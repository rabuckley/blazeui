using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Drawer;

/// <summary>
/// A positioning container for the drawer popup that can be made scrollable.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
/// <remarks>
/// In Base UI, the Viewport handles swipe-dismiss gestures and provides
/// scroll-vs-swipe arbitration. In BlazeUI, swipe gesture handling is
/// delegated to the JS module; the Viewport renders the appropriate data
/// attributes so that CSS transitions can target it.
/// </remarks>
public class DrawerViewport : BlazeElement<DrawerViewportState>
{
    [CascadingParameter] internal DrawerContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";

    protected override DrawerViewportState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-open", Context.Open ? "" : null);
        yield return new("data-closed", !Context.Open ? "" : null);
    }
}

public readonly record struct DrawerViewportState(bool Open);
