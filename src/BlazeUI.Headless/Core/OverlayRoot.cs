using BlazeUI.Bridge;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Core;

/// <summary>
/// Base class for overlay Root components (Dialog, Menu, Popover, etc.) that share
/// the same lifecycle: DI injections, <see cref="ComponentState{T}"/> open/close management,
/// JS module import, and teardown.
///
/// Subclasses provide the context object, JS module path, <see cref="BuildRenderTree"/>
/// (to cascade their internal context type), and any component-specific behavior via
/// abstract/virtual overrides. The <see cref="DotNetObjectReference{T}"/> stays in each
/// subclass because <c>[JSInvokable]</c> methods must live on the concrete type that the
/// reference wraps.
/// </summary>
public abstract class OverlayRoot : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private protected BlazeUIConfiguration Config { get; set; } = default!;
    [Inject] private protected BrowserMutationQueue MutationQueue { get; set; } = default!;

    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>Controlled open state. When set, the component is controlled.</summary>
    [Parameter] public bool? Open { get; set; }

    /// <summary>Callback fired when the open state changes.</summary>
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }

    /// <summary>Initial open state for uncontrolled mode.</summary>
    [Parameter] public bool DefaultOpen { get; set; }

    // Open state, exposed to subclasses via OpenValue.
    private protected readonly ComponentState<bool> _open = new(false);
    private protected bool OpenValue => _open.Value;

    /// <summary>
    /// The imported JS module. Subclasses reference this to push onto their context
    /// and to call JS methods (e.g. <c>hide</c>, <c>animateAndHide</c>).
    /// </summary>
    private protected IJSObjectReference? _jsModule;
    private bool _jsInitialized;

    // Subclass contract

    /// <summary>Path to the JS module file (e.g. <c>./_content/BlazeUI.Headless/js/dialog/dialog.js</c>).</summary>
    private protected abstract string ModulePath { get; }

    /// <summary>
    /// The key that identifies this component instance within the shared JS module scope.
    /// Passed to <c>dispose(key)</c> so the module cleans up only this instance's state.
    /// </summary>
    private protected abstract string JsInstanceKey { get; }

    /// <summary>Push current open state (and any other state) onto the context object.</summary>
    private protected abstract void SyncContextState();

    /// <summary>
    /// Called on first render after JS module import. Create <see cref="DotNetObjectReference{T}"/>
    /// and push JS refs onto the context here.
    /// </summary>
    private protected abstract Task OnJsInitializedAsync();

    /// <summary>Dispose <see cref="DotNetObjectReference{T}"/> and any other resources.</summary>
    private protected virtual void OnDispose() { }

    /// <summary>Extra parameter sync beyond open state (e.g. Select syncs value, Drawer syncs direction).</summary>
    private protected virtual void OnParametersSetCore() { }

    /// <summary>Post-JS-init work on subsequent renders (not commonly needed).</summary>
    private protected virtual Task OnAfterRenderCore(bool firstRender) => Task.CompletedTask;

    /// <summary>
    /// Called when closing, before <see cref="OpenChanged"/> fires.
    /// Write-through components override this to call JS <c>hide()</c> directly,
    /// bypassing Portal re-render propagation which is unreliable in Blazor Server mode.
    /// Mutations enqueued here are flushed by the <c>OnAfterRenderAsync</c> call that
    /// follows the <see cref="SetOpenAsync"/>-triggered <c>StateHasChanged()</c>.
    /// </summary>
    private protected virtual Task OnBeforeCloseAsync() => Task.CompletedTask;

    /// <summary>
    /// Update the open state, sync context, call <see cref="OnBeforeCloseAsync"/> on close,
    /// fire <see cref="OpenChanged"/>, and re-render. The <c>StateHasChanged()</c> call
    /// triggers the render cycle whose <c>OnAfterRenderAsync</c> flushes any mutations
    /// enqueued during <see cref="OnBeforeCloseAsync"/>. Components with bidirectional JS
    /// (e.g. ContextMenu) override this entirely.
    /// </summary>
    internal virtual async Task SetOpenAsync(bool value)
    {
        var wasOpen = _open.Value;
        _open.SetInternal(value);
        SyncContextState();

        if (!value && wasOpen)
            await OnBeforeCloseAsync();

        if (OpenChanged.HasDelegate)
            await OpenChanged.InvokeAsync(_open.Value);

        StateHasChanged();
    }

    // Lifecycle

    protected override void OnInitialized()
    {
        if (DefaultOpen) _open.SetInternal(true);
    }

    protected override void OnParametersSet()
    {
        if (Open.HasValue) _open.SetControlled(Open.Value);
        else _open.ClearControlled();

        SyncContextState();
        OnParametersSetCore();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", ModulePath.FormatUrl(Config));
            await OnJsInitializedAsync();
            _jsInitialized = true;
        }

        await OnAfterRenderCore(firstRender);
        await MutationQueue.FlushAsync();
    }

    // BuildRenderTree is left to subclasses — each cascades its own internal context type.

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        var module = _jsModule;
        _jsModule = null;

        try { if (module is not null && _jsInitialized) await module.InvokeVoidAsync("dispose", JsInstanceKey); }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }

        try { if (module is not null) await module.DisposeAsync(); }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }

        OnDispose();
    }
}
