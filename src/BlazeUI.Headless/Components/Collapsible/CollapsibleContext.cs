using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Collapsible;

internal sealed class CollapsibleContext
{
    public bool Open { get; set; }
    public bool Disabled { get; set; }
    public string TriggerId { get; set; } = "";
    public string PanelId { get; set; } = "";
    public Func<Task> Toggle { get; set; } = () => Task.CompletedTask;
    public IJSObjectReference? JsModule { get; set; }
    public DotNetObjectReference<CollapsibleRoot>? DotNetRef { get; set; }
}
