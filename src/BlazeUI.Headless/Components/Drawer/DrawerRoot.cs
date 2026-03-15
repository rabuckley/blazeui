using BlazeUI.Headless.Core;
using BlazeUI.Headless.Overlay.Mutations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Drawer;

/// <summary>
/// State container for a drawer. Groups all drawer sub-parts and manages open/close state.
/// Does not render its own HTML element — cascades <see cref="DrawerContext"/> to children.
/// </summary>
public class DrawerRoot : OverlayRoot
{
    /// <summary>
    /// The direction you swipe to dismiss the drawer.
    /// Defaults to <see cref="SwipeDirection.Down"/>, matching Base UI's default for
    /// bottom-anchored drawers.
    /// </summary>
    [Parameter] public SwipeDirection SwipeDirection { get; set; } = SwipeDirection.Down;

    /// <summary>
    /// Optional provider context for coordinating nested drawer state.
    /// When present, this drawer reports its open/close state to the provider.
    /// </summary>
    [CascadingParameter] internal DrawerProviderContext? ProviderContext { get; set; }

    private DotNetObjectReference<DrawerRoot>? _dotNetRef;
    private readonly DrawerContext _context;

    private protected override string ModulePath => "./_content/BlazeUI.Headless/js/drawer/drawer.js";
    private protected override string JsInstanceKey => _context.PopupId;

    public DrawerRoot()
    {
        _context = new DrawerContext { PopupId = IdGenerator.Next("drawer-popup") };
        _context.SetOpen = SetOpenAsync;
        _context.Close = () => SetOpenAsync(false);
    }

    private protected override void SyncContextState()
    {
        _context.Open = OpenValue;
        _context.SwipeDirection = SwipeDirection;

        // Report state changes to the provider for indent/background coordination.
        ProviderContext?.SetDrawerOpen(_context.PopupId, OpenValue);
    }

    private protected override Task OnJsInitializedAsync()
    {
        _dotNetRef = DotNetObjectReference.Create(this);
        _context.JsModule = _jsModule;
        _context.DotNetRef = _dotNetRef;

        if (OpenValue)
            MutationQueue.Enqueue(new ShowPopoverMutation
            {
                ElementId = _context.PopupId,
                JsModule = _jsModule!,
                DotNetRef = _dotNetRef,
            });

        return Task.CompletedTask;
    }

    private protected override void OnDispose()
    {
        ProviderContext?.RemoveDrawer(_context.PopupId);
        _dotNetRef?.Dispose();
    }

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

    [JSInvokable] public Task OnEscapeKey() => SetOpenAsync(false);
    [JSInvokable] public Task OnExitAnimationComplete() { StateHasChanged(); return Task.CompletedTask; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<DrawerContext>>(0);
        builder.AddComponentParameter(1, "Value", _context);
        builder.AddComponentParameter(2, "ChildContent", ChildContent);
        builder.CloseComponent();
    }
}
