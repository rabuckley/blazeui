using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Menu;

internal sealed class MenuContext
{
    public bool Open { get; set; }
    public string? HighlightedItemId { get; set; }
    public string TriggerId { get; set; } = "";
    public string PopupId { get; set; } = "";
    public Func<bool, Task> SetOpen { get; set; } = _ => Task.CompletedTask;
    public Func<Task> Close { get; set; } = () => Task.CompletedTask;
    public IJSObjectReference? JsModule { get; set; }

    /// <summary>
    /// .NET reference passed to JS for callbacks. For the root this is
    /// <see cref="DotNetObjectReference{MenuRoot}"/>; for submenus it is
    /// <see cref="DotNetObjectReference{MenuSubmenuRoot}"/>. Typed as <c>object</c>
    /// because JS interop serialises any <c>DotNetObjectReference&lt;T&gt;</c> the same way.
    /// </summary>
    public object? DotNetRef { get; set; }

    // Submenu coordination — when a submenu opens, it registers a close callback here
    // so sibling submenu triggers can close it before opening their own submenu.
    public Func<Task>? CloseOpenSubmenu { get; set; }

    /// <summary>
    /// Set by the Root after <c>OnExitAnimationComplete</c>. Used by
    /// <see cref="MenuPortal"/> to deactivate the portal entry so closed
    /// overlay content doesn't leave empty containers in the DOM.
    /// Reset to false when the menu opens.
    /// </summary>
    public bool ExitComplete { get; set; }

    // Placement info (set by MenuPositioner so MenuPopup can emit data-side/data-align)
    public string? PlacementSide { get; set; }
    public string? PlacementAlign { get; set; }
}
