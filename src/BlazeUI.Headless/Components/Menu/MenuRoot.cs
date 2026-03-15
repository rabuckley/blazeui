using BlazeUI.Headless.Core;
using BlazeUI.Headless.Overlay.Mutations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Menu;

/// <summary>
/// State container for a menu. Click-triggered, keyboard navigable, with typeahead.
/// Owns JS module for keyboard nav, click-outside, positioning, and submenu logic.
/// </summary>
public class MenuRoot : OverlayRoot
{
    private DotNetObjectReference<MenuRoot>? _dotNetRef;
    private readonly MenuContext _context;

    private protected override string ModulePath => "./_content/BlazeUI.Headless/js/menu/menu.js";
    private protected override string JsInstanceKey => _context.PopupId;

    public MenuRoot()
    {
        _context = new MenuContext
        {
            TriggerId = IdGenerator.Next("menu-trigger"),
            PopupId = IdGenerator.Next("menu-popup"),
        };
        _context.SetOpen = SetOpenAsync;
        _context.Close = () => SetOpenAsync(false);
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

    /// <summary>
    /// Clear highlight state and cascade close to any open submenus before hiding the root popup.
    /// Submenus are separate top-layer elements that must be hidden independently.
    /// </summary>
    internal override async Task SetOpenAsync(bool value)
    {
        if (value)
            _context.ExitComplete = false;

        if (!value)
        {
            _context.HighlightedItemId = null;

            // Close the open submenu chain (deepest first) so their JS popups are
            // hidden before the root popup disappears from under them.
            if (_context.CloseOpenSubmenu is not null)
                await _context.CloseOpenSubmenu();
        }
        await base.SetOpenAsync(value);
    }

    [JSInvokable] public Task OnClickOutside() => SetOpenAsync(false);
    [JSInvokable] public Task OnEscapeKey() => SetOpenAsync(false);

    [JSInvokable]
    public Task OnExitAnimationComplete()
    {
        _context.ExitComplete = true;
        StateHasChanged();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnHighlightChange(string? itemId)
    {
        _context.HighlightedItemId = itemId;
        StateHasChanged();
        return Task.CompletedTask;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<MenuContext>>(0);
        builder.AddComponentParameter(1, "Value", _context);
        builder.AddComponentParameter(2, "ChildContent", ChildContent);
        builder.CloseComponent();
    }
}
