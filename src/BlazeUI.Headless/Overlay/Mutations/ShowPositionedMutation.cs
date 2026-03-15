using BlazeUI.Bridge;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Overlay.Mutations;

/// <summary>
/// Calls <c>show(anchorId, popupId, options, dotNetRef)</c> on the component's JS module
/// to open and position a floating popup relative to an anchor element.
/// Used by Menu, Popover, Select, and Autocomplete positioners.
/// </summary>
internal sealed class ShowPositionedMutation : BrowserMutation
{
    public required IJSObjectReference JsModule { get; init; }
    public required string AnchorId { get; init; }
    public required object Options { get; init; }
    public required object DotNetRef { get; init; }

    public override async Task ExecuteAsync()
    {
        await JsModule.InvokeVoidAsync("show", AnchorId, ElementId, Options, DotNetRef);
    }
}
