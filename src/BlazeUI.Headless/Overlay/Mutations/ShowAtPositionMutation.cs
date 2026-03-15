using BlazeUI.Bridge;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Overlay.Mutations;

/// <summary>
/// Calls <c>showAtPosition(popupId, x, y, dotNetRef, positionerId)</c> on the component's
/// JS module to open a popup at absolute cursor coordinates. Used by ContextMenu.
/// The positioner element receives positioning styles; the popup receives data attributes.
/// </summary>
internal sealed class ShowAtPositionMutation : BrowserMutation
{
    public required IJSObjectReference JsModule { get; init; }
    public required double X { get; init; }
    public required double Y { get; init; }
    public required object DotNetRef { get; init; }
    public string? PositionerId { get; init; }

    public override async Task ExecuteAsync()
    {
        await JsModule.InvokeVoidAsync("showAtPosition", ElementId, X, Y, DotNetRef, PositionerId);
    }
}
