using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.ContextMenu;

/// <summary>
/// Context for ContextMenu. Extends the base menu context with cursor position
/// for virtual anchor positioning.
/// </summary>
internal sealed class ContextMenuContext
{
    public bool Open { get; set; }
    public bool Disabled { get; set; }
    public string PopupId { get; set; } = "";
    public string PositionerId { get; set; } = "";
    public double CursorX { get; set; }
    public double CursorY { get; set; }
    public string? HighlightedItemId { get; set; }
    public Func<bool, Task> SetOpen { get; set; } = _ => Task.CompletedTask;
    public Func<Task> Close { get; set; } = () => Task.CompletedTask;
    public IJSObjectReference? JsModule { get; set; }
    public DotNetObjectReference<ContextMenuRoot>? DotNetRef { get; set; }

    // Submenu coordination — when a submenu opens, it registers a close callback here
    // so sibling submenu triggers can close it before opening their own submenu.
    public Func<Task>? CloseOpenSubmenu { get; set; }

    // Radio group support (mirrors MenuContext)
    public string? RadioGroupValue { get; set; }
    public Func<string, Task>? OnRadioGroupChange { get; set; }
}
