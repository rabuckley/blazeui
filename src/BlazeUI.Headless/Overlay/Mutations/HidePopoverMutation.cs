using BlazeUI.Bridge;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Overlay.Mutations;

/// <summary>
/// Calls <c>hide(elementId)</c> on the component's JS module
/// to dismiss a popup element from the top layer.
/// </summary>
internal sealed class HidePopoverMutation : BrowserMutation
{
    public required IJSObjectReference JsModule { get; init; }

    public override async Task ExecuteAsync()
    {
        await JsModule.InvokeVoidAsync("hide", ElementId);
    }
}
