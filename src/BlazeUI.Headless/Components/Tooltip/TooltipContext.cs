using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Tooltip;

internal sealed class TooltipContext
{
    public bool Open { get; set; }

    /// <summary>
    /// Whether the tooltip is disabled. When true, triggers set <c>data-trigger-disabled</c>
    /// and the JS hover/focus events are suppressed.
    /// </summary>
    public bool Disabled { get; set; }

    public string TriggerId { get; set; } = "";
    public string PopupId { get; set; } = "";
    public string PositionerId { get; set; } = "";

    /// <summary>Resolved side from the positioner, propagated to popup and arrow for <c>data-side</c>.</summary>
    public string Side { get; set; } = "top";

    /// <summary>Resolved alignment from the positioner, propagated to popup and arrow for <c>data-align</c>.</summary>
    public string Align { get; set; } = "center";

    public Func<bool, Task> SetOpen { get; set; } = _ => Task.CompletedTask;

    /// <summary>
    /// The JS module loaded by TooltipRoot, shared with child components
    /// that need to call JS interop (e.g., positioning, animation).
    /// </summary>
    public IJSObjectReference? JsModule { get; set; }

    public DotNetObjectReference<TooltipRoot>? DotNetRef { get; set; }
}
