using BlazeUI.Headless.Core;
using BlazeUI.Headless.Overlay.Mutations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.PreviewCard;

/// <summary>
/// State container for a preview card. Hover-triggered like Tooltip but with
/// interactive content and a longer default open delay (600 ms).
/// </summary>
public class PreviewCardRoot : OverlayRoot
{
    /// <summary>Delay in ms before showing on hover/focus.</summary>
    [Parameter] public int Delay { get; set; } = 600;

    /// <summary>Delay in ms before hiding after pointer/focus leaves.</summary>
    [Parameter] public int CloseDelay { get; set; } = 300;

    private DotNetObjectReference<PreviewCardRoot>? _dotNetRef;
    private readonly PreviewCardContext _context;

    private protected override string ModulePath => "./_content/BlazeUI.Headless/js/previewcard/previewcard.js";
    private protected override string JsInstanceKey => _context.TriggerId;

    public PreviewCardRoot()
    {
        _context = new PreviewCardContext
        {
            TriggerId = IdGenerator.Next("previewcard-trigger"),
            PopupId = IdGenerator.Next("previewcard-popup"),
            PositionerId = IdGenerator.Next("previewcard-positioner"),
        };
        _context.SetOpen = SetOpenAsync;
    }

    private protected override void SyncContextState() => _context.Open = OpenValue;

    private protected override async Task OnJsInitializedAsync()
    {
        _dotNetRef = DotNetObjectReference.Create(this);
        _context.JsModule = _jsModule;
        _context.DotNetRef = _dotNetRef;

        await _jsModule!.InvokeVoidAsync("init", new
        {
            triggerId = _context.TriggerId,
            popupId = _context.PopupId,
            enterDelay = Delay,
            exitDelay = CloseDelay,
        }, _dotNetRef);
    }

    private protected override void OnDispose() => _dotNetRef?.Dispose();

    /// <summary>
    /// Write-through close: enqueue <see cref="AnimateAndHideMutation"/> to run the exit
    /// animation on the positioner. Flushed by the render cycle triggered by
    /// <see cref="OverlayRoot.SetOpenAsync"/>'s <c>StateHasChanged()</c>.
    /// </summary>
    private protected override Task OnBeforeCloseAsync()
    {
        if (_jsModule is not null)
            MutationQueue.Enqueue(new AnimateAndHideMutation
            {
                ElementId = _context.PositionerId,
                TriggerId = _context.TriggerId,
                JsModule = _jsModule,
            });
        return Task.CompletedTask;
    }

    [JSInvokable] public Task OnHoverEnter() => SetOpenAsync(true);
    [JSInvokable] public Task OnHoverExit() => SetOpenAsync(false);

    [JSInvokable]
    public Task OnExitAnimationComplete()
    {
        StateHasChanged();
        return Task.CompletedTask;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<PreviewCardContext>>(0);
        builder.AddComponentParameter(1, "Value", _context);
        builder.AddComponentParameter(2, "ChildContent", ChildContent);
        builder.CloseComponent();
    }
}
