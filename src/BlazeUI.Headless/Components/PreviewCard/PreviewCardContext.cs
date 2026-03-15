using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.PreviewCard;

internal sealed class PreviewCardContext
{
    public bool Open { get; set; }
    public string TriggerId { get; set; } = "";
    public string PopupId { get; set; } = "";
    public string PositionerId { get; set; } = "";
    public string? TitleId { get; set; }
    public string? DescriptionId { get; set; }

    /// <summary>Resolved side from the positioner, propagated to popup and arrow for <c>data-side</c>.</summary>
    public string Side { get; set; } = "bottom";

    /// <summary>Resolved alignment from the positioner, propagated to popup and arrow for <c>data-align</c>.</summary>
    public string Align { get; set; } = "center";

    public Func<bool, Task> SetOpen { get; set; } = _ => Task.CompletedTask;
    public IJSObjectReference? JsModule { get; set; }
    public DotNetObjectReference<PreviewCardRoot>? DotNetRef { get; set; }
}
