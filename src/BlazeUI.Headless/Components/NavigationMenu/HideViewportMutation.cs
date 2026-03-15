using BlazeUI.Bridge;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.NavigationMenu;

/// <summary>
/// Calls <c>hide(viewportId, dotNetRef)</c> on the navigation menu JS module
/// to dismiss the viewport with an exit animation callback.
/// </summary>
internal sealed class HideViewportMutation : BrowserMutation
{
    public required IJSObjectReference JsModule { get; init; }
    public required object DotNetRef { get; init; }

    public override async Task ExecuteAsync()
    {
        await JsModule.InvokeVoidAsync("hide", ElementId, DotNetRef);
    }
}
