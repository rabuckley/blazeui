using BlazeUI.Headless.Components.Dialog;
using BlazeUI.Headless.Core;
using BlazeUI.Headless.Overlay.Mutations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.AlertDialog;

/// <summary>
/// State container for an alert dialog. Like <see cref="DialogRoot"/>
/// but with no click-outside dismissal — users must close via an action
/// button or the Escape key. Modal is always enforced.
/// </summary>
/// <remarks>
/// Cascades <see cref="DialogContext"/> (with <c>Role = "alertdialog"</c>)
/// so all Dialog sub-components (Popup, Backdrop, Close, Trigger, Title,
/// Description, Portal) can be reused — matching the Base UI architecture
/// where AlertDialog shares Dialog's sub-parts.
/// </remarks>
public class AlertDialogRoot : OverlayRoot
{
    private DotNetObjectReference<AlertDialogRoot>? _dotNetRef;
    private readonly DialogContext _context;

    private protected override string ModulePath => "./_content/BlazeUI.Headless/js/alertdialog/alertdialog.js";
    private protected override string JsInstanceKey => _context.PopupId;

    public AlertDialogRoot()
    {
        _context = new DialogContext
        {
            PopupId = IdGenerator.Next("alertdialog-popup"),
            Role = "alertdialog",
            TitleId = IdGenerator.Next("alertdialog-title"),
            DescriptionId = IdGenerator.Next("alertdialog-description"),
        };
        _context.SetOpen = SetOpenAsync;
        _context.Close = () => SetOpenAsync(false);
    }

    private protected override void SyncContextState()
    {
        _context.Open = OpenValue;
        _context.Modal = true;
        _context.NestingLevel = (ParentDialogContext?.NestingLevel ?? 0) + 1;
    }

    /// <summary>
    /// Receives the parent dialog's context when nested inside another
    /// dialog or alert dialog.
    /// </summary>
    [CascadingParameter]
    internal DialogContext? ParentDialogContext { get; set; }

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

    private protected override void OnDispose() => _dotNetRef?.Dispose();

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

    [JSInvokable]
    public Task OnExitAnimationComplete()
    {
        StateHasChanged();
        return Task.CompletedTask;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<DialogContext>>(0);
        builder.AddComponentParameter(1, "Value", _context);
        builder.AddComponentParameter(2, "ChildContent", ChildContent);
        builder.CloseComponent();
    }
}
