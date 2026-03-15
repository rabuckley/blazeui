using BlazeUI.Headless.Core;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Drawer;

internal sealed class DrawerContext
{
    public bool Open { get; set; }

    // Base UI swipeDirection: the direction you swipe to *dismiss* the drawer.
    // Defaults to 'down' (matching Base UI's default for bottom-anchored drawers).
    public SwipeDirection SwipeDirection { get; set; } = SwipeDirection.Down;

    public string PopupId { get; set; } = "";
    public string? TitleId { get; set; }
    public string? DescriptionId { get; set; }
    public Func<bool, Task> SetOpen { get; set; } = _ => Task.CompletedTask;
    public Func<Task> Close { get; set; } = () => Task.CompletedTask;
    public IJSObjectReference? JsModule { get; set; }
    public DotNetObjectReference<DrawerRoot>? DotNetRef { get; set; }
}
