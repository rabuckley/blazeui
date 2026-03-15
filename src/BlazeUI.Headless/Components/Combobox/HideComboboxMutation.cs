using BlazeUI.Bridge;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Combobox;

/// <summary>
/// Calls <c>hide(popupId, positionerId)</c> on the combobox JS module to dismiss
/// the popup with exit animation and then hide the positioner's popover.
/// </summary>
internal sealed class HideComboboxMutation : BrowserMutation
{
    public required IJSObjectReference JsModule { get; init; }
    public required string PositionerId { get; init; }

    public override async Task ExecuteAsync()
    {
        await JsModule.InvokeVoidAsync("hide", ElementId, PositionerId);
    }
}
