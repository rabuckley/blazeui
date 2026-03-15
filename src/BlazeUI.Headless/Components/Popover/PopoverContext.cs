using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Popover;

internal sealed class PopoverContext
{
    public bool Open { get; set; }
    public string TriggerId { get; set; } = "";
    public string PopupId { get; set; } = "";
    public string? TitleId { get; set; }
    public string? DescriptionId { get; set; }
    public Func<bool, Task> SetOpen { get; set; } = _ => Task.CompletedTask;
    public IJSObjectReference? JsModule { get; set; }
    public DotNetObjectReference<PopoverRoot>? DotNetRef { get; set; }
}
