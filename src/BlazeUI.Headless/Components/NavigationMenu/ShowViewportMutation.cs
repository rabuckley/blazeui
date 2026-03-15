using BlazeUI.Bridge;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.NavigationMenu;

/// <summary>
/// Calls <c>show(viewportId, contentId)</c> on the navigation menu JS module
/// to reveal the viewport with the active item's content.
/// </summary>
internal sealed class ShowViewportMutation : BrowserMutation
{
    public required IJSObjectReference JsModule { get; init; }

    /// <summary>
    /// The DOM element ID of the active content panel. The JS module
    /// looks it up via <c>getElementById</c> to measure its dimensions
    /// for viewport size transitions.
    /// </summary>
    public required string? ContentId { get; init; }

    public override async Task ExecuteAsync()
    {
        await JsModule.InvokeVoidAsync("show", ElementId, ContentId);
    }
}
