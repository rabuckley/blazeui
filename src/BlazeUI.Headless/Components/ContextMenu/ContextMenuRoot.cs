using BlazeUI.Headless.Core;
using BlazeUI.Headless.Overlay.Mutations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.ContextMenu;

/// <summary>
/// State container for a context menu. Triggered by <c>contextmenu</c> event (right-click).
/// Popup is positioned at cursor coordinates via virtual anchor.
/// Reuses Menu's JS module for keyboard nav, click-outside, etc.
/// </summary>
public class ContextMenuRoot : OverlayRoot
{
    private DotNetObjectReference<ContextMenuRoot>? _dotNetRef;
    private readonly ContextMenuContext _context;

    // Reuse Menu's JS module — it has showAtPosition() for virtual anchor.
    private protected override string ModulePath => "./_content/BlazeUI.Headless/js/menu/menu.js";
    private protected override string JsInstanceKey => _context.PopupId;

    /// <summary>Whether the component should ignore user interaction.</summary>
    [Parameter] public bool Disabled { get; set; }

    public ContextMenuRoot()
    {
        _context = new ContextMenuContext
        {
            PopupId = IdGenerator.Next("contextmenu-popup"),
            PositionerId = IdGenerator.Next("contextmenu-positioner"),
        };
        _context.SetOpen = SetOpenAsync;
        _context.Close = () => SetOpenAsync(false);
    }

    private protected override void SyncContextState()
    {
        _context.Open = OpenValue;
        _context.Disabled = Disabled;
    }

    private protected override Task OnJsInitializedAsync()
    {
        _dotNetRef = DotNetObjectReference.Create(this);
        _context.JsModule = _jsModule;
        _context.DotNetRef = _dotNetRef;
        return Task.CompletedTask;
    }

    private protected override void OnDispose() => _dotNetRef?.Dispose();

    internal async Task OpenAtPositionAsync(double x, double y)
    {
        _context.CursorX = x;
        _context.CursorY = y;
        await SetOpenAsync(true);
    }

    /// <summary>
    /// Full override: ContextMenu calls JS for both open (<c>showAtPosition</c>) and close
    /// (<c>hide</c>), so it bypasses the base <c>OnBeforeCloseAsync</c> pattern entirely.
    /// </summary>
    internal override async Task SetOpenAsync(bool value)
    {
        // Ignore open requests when disabled.
        if (value && Disabled) return;

        var wasOpen = _open.Value;
        _open.SetInternal(value);
        SyncContextState();
        if (!value) _context.HighlightedItemId = null;

        if (_jsModule is not null)
        {
            if (value && !wasOpen)
                MutationQueue.Enqueue(new ShowAtPositionMutation
                {
                    ElementId = _context.PopupId,
                    PositionerId = _context.PositionerId,
                    JsModule = _jsModule,
                    X = _context.CursorX,
                    Y = _context.CursorY,
                    DotNetRef = _dotNetRef!,
                });
            else if (!value && wasOpen)
                MutationQueue.Enqueue(new HidePopoverMutation
                {
                    ElementId = _context.PopupId,
                    JsModule = _jsModule,
                });
        }

        if (OpenChanged.HasDelegate)
            await OpenChanged.InvokeAsync(_open.Value);
        StateHasChanged();
    }

    [JSInvokable] public Task OnClickOutside() => SetOpenAsync(false);
    [JSInvokable] public Task OnEscapeKey() => SetOpenAsync(false);
    [JSInvokable] public Task OnExitAnimationComplete() { StateHasChanged(); return Task.CompletedTask; }

    [JSInvokable]
    public Task OnHighlightChange(string? itemId)
    {
        _context.HighlightedItemId = itemId;
        StateHasChanged();
        return Task.CompletedTask;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<ContextMenuContext>>(0);
        builder.AddComponentParameter(1, "Value", _context);
        builder.AddComponentParameter(2, "ChildContent", ChildContent);
        builder.CloseComponent();
    }
}
