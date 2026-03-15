using BlazeUI.Bridge;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Overlay.Mutations;

/// <summary>
/// Calls <c>show(elementId, dotNetRef)</c> on the component's JS module
/// to promote a <c>&lt;dialog&gt;</c> or popup element to the top layer.
/// </summary>
internal sealed class ShowPopoverMutation : BrowserMutation
{
    public required IJSObjectReference JsModule { get; init; }

    /// <summary>
    /// The <see cref="DotNetObjectReference{T}"/> for JS → .NET callbacks.
    /// Typed as <c>object</c> because each root component has a different <c>T</c>
    /// but the JS <c>show()</c> signature is uniform.
    /// </summary>
    public required object DotNetRef { get; init; }

    public override async Task ExecuteAsync()
    {
        await JsModule.InvokeVoidAsync("show", ElementId, DotNetRef);
    }
}
