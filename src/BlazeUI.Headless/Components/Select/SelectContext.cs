using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Select;

internal sealed class SelectContext
{
    public bool Open { get; set; }
    public string? SelectedValue { get; set; }
    public string? SelectedLabel { get; set; }
    public string? HighlightedItemId { get; set; }
    public string TriggerId { get; set; } = "";
    public string PopupId { get; set; } = "";
    public string? LabelId { get; set; }
    public string? ListId { get; set; }
    public string? Placeholder { get; set; }
    public bool Disabled { get; set; }
    public bool ReadOnly { get; set; }
    public bool Required { get; set; }
    public Func<bool, Task> SetOpen { get; set; } = _ => Task.CompletedTask;
    public Func<Task> Close { get; set; } = () => Task.CompletedTask;
    public Func<string, string?, Task> SelectItem { get; set; } = (_, _) => Task.CompletedTask;
    public IJSObjectReference? JsModule { get; set; }
    public DotNetObjectReference<SelectRoot>? DotNetRef { get; set; }
}
