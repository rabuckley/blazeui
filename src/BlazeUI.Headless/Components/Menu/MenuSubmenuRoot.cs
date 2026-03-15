using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Menu;

/// <summary>
/// Nested state container for submenus. Cascades its own context for submenu open state.
/// Owns its own <see cref="DotNetObjectReference{T}"/> so JS callbacks (Escape, click-outside,
/// exit animation) target this submenu rather than the root.
/// </summary>
public class MenuSubmenuRoot : ComponentBase, IDisposable
{
    [CascadingParameter] internal MenuContext ParentContext { get; set; } = default!;

    [Parameter] public RenderFragment? ChildContent { get; set; }

    private readonly ComponentState<bool> _open = new(false);
    private readonly MenuContext _context;
    private DotNetObjectReference<MenuSubmenuRoot>? _dotNetRef;

    // Guard against re-opening during the async close sequence. When the submenu
    // closes, SetOpenAsync(false) awaits JS hide(). During that await, Blazor may
    // process queued pointer events on the trigger (which is still under the cursor),
    // causing a re-entrant SetOpenAsync(true) call that reopens the submenu.
    private bool _closing;

    public MenuSubmenuRoot()
    {
        _context = new MenuContext
        {
            TriggerId = IdGenerator.Next("submenu-trigger"),
            PopupId = IdGenerator.Next("submenu-popup"),
        };
        _context.SetOpen = SetOpenAsync;
        _context.Close = () => SetOpenAsync(false);
    }

    private async Task SetOpenAsync(bool value)
    {
        if (_open.Value == value) return;
        if (value && _closing) return;

        if (value)
        {
            // Close any sibling submenu before opening this one.
            if (ParentContext.CloseOpenSubmenu is not null)
                await ParentContext.CloseOpenSubmenu();

            ParentContext.CloseOpenSubmenu = () => SetOpenAsync(false);
        }
        else
        {
            _closing = true;
            ParentContext.CloseOpenSubmenu = null;

            // Close any open child submenu first (recursive cascade — deepest closes first).
            if (_context.CloseOpenSubmenu is not null)
                await _context.CloseOpenSubmenu();

            // Hide the popup via JS. Submenu popups are separate top-layer elements
            // that must be hidden independently — the root's hide only targets the
            // root popup. This mirrors how MenuRoot.OnBeforeCloseAsync enqueues
            // HidePopoverMutation for its own popup.
            if (_context.JsModule is not null)
            {
                try { await _context.JsModule.InvokeVoidAsync("hide", _context.PopupId); }
                catch (JSDisconnectedException) { }
                catch (OperationCanceledException) { }
            }

            _closing = false;
        }

        _open.SetInternal(value);
        _context.Open = _open.Value;
        StateHasChanged();
    }

    protected override void OnParametersSet()
    {
        // Inherit JS module from parent for positioning/animation.
        _context.JsModule = ParentContext.JsModule;

        // Each submenu owns its own DotNetRef so JS callbacks (Escape, click-outside)
        // target this submenu rather than the root. Created lazily because the
        // constructor runs before DI is available.
        _dotNetRef ??= DotNetObjectReference.Create(this);
        _context.DotNetRef = _dotNetRef;
    }

    // JS callbacks — each targets this submenu, not the root.

    [JSInvokable]
    public Task OnClickOutside() => SetOpenAsync(false);

    [JSInvokable]
    public Task OnEscapeKey() => SetOpenAsync(false);

    [JSInvokable]
    public Task OnExitAnimationComplete()
    {
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

    public void Dispose()
    {
        _dotNetRef?.Dispose();
    }
}
