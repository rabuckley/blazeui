using BlazeUI.Headless.Core;
using BlazeUI.Headless.Overlay.Mutations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Popover;

/// <summary>
/// State container for a popover. No DOM element — cascades context and owns JS module.
/// Click-triggered, focus-trapped, click-outside-dismissable.
/// </summary>
public class PopoverRoot : OverlayRoot
{
    private DotNetObjectReference<PopoverRoot>? _dotNetRef;
    private readonly PopoverContext _context;

    private protected override string ModulePath => "./_content/BlazeUI.Headless/js/popover/popover.js";
    private protected override string JsInstanceKey => _context.PopupId;

    public PopoverRoot()
    {
        _context = new PopoverContext
        {
            TriggerId = IdGenerator.Next("popover-trigger"),
            PopupId = IdGenerator.Next("popover-popup"),
        };
        _context.SetOpen = SetOpenAsync;
    }

    private protected override void SyncContextState() => _context.Open = OpenValue;

    private protected override Task OnJsInitializedAsync()
    {
        _dotNetRef = DotNetObjectReference.Create(this);
        _context.JsModule = _jsModule;
        _context.DotNetRef = _dotNetRef;
        return Task.CompletedTask;
    }

    private protected override void OnDispose() => _dotNetRef?.Dispose();

    /// <summary>
    /// Enqueue JS <c>hide()</c> when closing. Flushed in <see cref="OverlayRoot"/>'s
    /// <c>OnAfterRenderAsync</c>. This bypasses Portal re-render propagation which
    /// is unreliable in Blazor Server mode (cascading value reference equality).
    /// </summary>
    private protected override Task OnBeforeCloseAsync()
    {
        if (_jsModule is not null)
            MutationQueue.Enqueue(new HidePopoverMutation
            {
                ElementId = _context.PopupId,
                JsModule = _jsModule,
            });
        return Task.CompletedTask;
    }

    [JSInvokable] public Task OnClickOutside() => SetOpenAsync(false);
    [JSInvokable] public Task OnEscapeKey() => SetOpenAsync(false);

    [JSInvokable]
    public Task OnExitAnimationComplete()
    {
        StateHasChanged();
        return Task.CompletedTask;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<PopoverContext>>(0);
        builder.AddComponentParameter(1, "Value", _context);
        builder.AddComponentParameter(2, "ChildContent", ChildContent);
        builder.CloseComponent();
    }
}
