using BlazeUI.Bridge;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Overlay.Mutations;

/// <summary>
/// Calls <c>animateAndHide(triggerId, popupId)</c> on the component's JS module
/// to run the exit animation and then dismiss the popup. Used by Tooltip and
/// PreviewCard which have a separate animate-and-hide step.
/// </summary>
internal sealed class AnimateAndHideMutation : BrowserMutation
{
    public required IJSObjectReference JsModule { get; init; }

    /// <summary>
    /// The trigger element ID, used as the JS instance key to look up per-instance state.
    /// </summary>
    public required string TriggerId { get; init; }

    public override async Task ExecuteAsync()
    {
        await JsModule.InvokeVoidAsync("animateAndHide", TriggerId, ElementId);
    }
}
