using BlazeUI.Bridge;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Combobox;

/// <summary>
/// Calls <c>show(inputId, popupId, positionerId, options, dotNetRef)</c> on the combobox JS module
/// to position the positioner relative to the input and promote it to the top layer.
/// </summary>
internal sealed class ShowComboboxMutation : BrowserMutation
{
    public required IJSObjectReference JsModule { get; init; }
    public required string InputId { get; init; }
    public required string PositionerId { get; init; }
    public required object DotNetRef { get; init; }
    public required string Placement { get; init; }
    public required int Offset { get; init; }
    public bool InlineComplete { get; init; }

    public override async Task ExecuteAsync()
    {
        await JsModule.InvokeVoidAsync("show", InputId, ElementId, PositionerId,
            new { placement = Placement, offset = Offset, inlineComplete = InlineComplete }, DotNetRef);
    }
}
