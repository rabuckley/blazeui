using BlazeUI.Headless.Core;
using BlazeUI.Headless.Overlay.Mutations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Tooltip;

/// <summary>
/// State container for a tooltip. Renders no DOM element — only cascades
/// context to child components and owns the JS module lifecycle.
/// </summary>
public class TooltipRoot : OverlayRoot
{
    /// <summary>Delay in ms before showing on hover/focus.</summary>
    [Parameter] public int Delay { get; set; } = 300;

    /// <summary>Delay in ms before hiding after pointer/focus leaves.</summary>
    [Parameter] public int CloseDelay { get; set; }

    /// <summary>
    /// When true, the tooltip will not open in response to hover or focus events.
    /// Does not disable the trigger element itself.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Optional provider context cascaded by <see cref="TooltipProvider"/>.
    /// When present, this tooltip defers to the provider's delay values and
    /// participates in grouped instant-open behavior.
    /// </summary>
    [CascadingParameter] internal TooltipProviderContext? ProviderContext { get; set; }

    private DotNetObjectReference<TooltipRoot>? _dotNetRef;
    private readonly TooltipContext _context;

    private protected override string ModulePath => "./_content/BlazeUI.Headless/js/tooltip/tooltip.js";
    private protected override string JsInstanceKey => _context.TriggerId;

    public TooltipRoot()
    {
        _context = new TooltipContext
        {
            TriggerId = IdGenerator.Next("tooltip-trigger"),
            PopupId = IdGenerator.Next("tooltip-popup"),
            PositionerId = IdGenerator.Next("tooltip-positioner"),
        };
        _context.SetOpen = SetOpenAsync;
    }

    /// <summary>
    /// Resolved open delay: provider delay if present, otherwise this tooltip's <see cref="Delay"/>.
    /// When the provider group is within its timeout window, returns 0 for instant open.
    /// </summary>
    private int ResolvedEnterDelay
    {
        get
        {
            if (ProviderContext?.IsWithinGroupTimeout() is true)
                return 0;
            return ProviderContext?.Delay ?? Delay;
        }
    }

    /// <summary>
    /// Resolved close delay: provider close delay if present, otherwise this tooltip's <see cref="CloseDelay"/>.
    /// </summary>
    private int ResolvedExitDelay => ProviderContext?.CloseDelay ?? CloseDelay;

    private protected override void SyncContextState()
    {
        _context.Open = OpenValue;
        _context.Disabled = Disabled;
    }

    private protected override async Task OnJsInitializedAsync()
    {
        _dotNetRef = DotNetObjectReference.Create(this);
        _context.JsModule = _jsModule;
        _context.DotNetRef = _dotNetRef;

        await _jsModule!.InvokeVoidAsync("init", new
        {
            triggerId = _context.TriggerId,
            popupId = _context.PopupId,
            enterDelay = ResolvedEnterDelay,
            exitDelay = ResolvedExitDelay,
        }, _dotNetRef);
    }

    private protected override void OnDispose() => _dotNetRef?.Dispose();

    /// <summary>
    /// Write-through close: enqueue <see cref="AnimateAndHideMutation"/> to run
    /// the exit animation. Flushed by the render cycle triggered by
    /// <see cref="OverlayRoot.SetOpenAsync"/>'s <c>StateHasChanged()</c>.
    /// </summary>
    private protected override Task OnBeforeCloseAsync()
    {
        // Record close timestamp on the provider for group delay coordination.
        ProviderContext?.RecordClose();

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
        builder.OpenComponent<CascadingValue<TooltipContext>>(0);
        builder.AddComponentParameter(1, "Value", _context);
        builder.AddComponentParameter(2, "ChildContent", ChildContent);
        builder.CloseComponent();
    }
}
