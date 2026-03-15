using BlazeUI.Headless.Core;
using BlazeUI.Headless.Overlay.Mutations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Dialog;

/// <summary>
/// State container for a dialog. Uses native <c>&lt;dialog&gt;</c> via <c>showModal()</c>
/// for top-layer promotion and inert. No DOM element — cascades context.
/// </summary>
public class DialogRoot : OverlayRoot
{
    private DotNetObjectReference<DialogRoot>? _dotNetRef;
    private readonly DialogContext _context;

    /// <summary>
    /// Receives the parent dialog's context when this dialog is nested inside
    /// another. Used to compute <see cref="DialogContext.NestingLevel"/> so
    /// <see cref="DialogBackdrop"/> can suppress nested backdrops.
    /// </summary>
    [CascadingParameter]
    internal DialogContext? ParentDialogContext { get; set; }

    /// <summary>
    /// Whether the dialog is modal. When <c>true</c> (the default), the native
    /// <c>showModal()</c> API is used to promote the dialog to the top layer and
    /// make the rest of the page inert. When <c>false</c>, a non-modal dialog is
    /// rendered instead.
    /// </summary>
    /// <remarks>
    /// The <c>'trap-focus'</c> variant from Base UI (focus trapped but no scroll
    /// lock or pointer blocking) is not yet supported — only <c>true</c> and
    /// <c>false</c> are honoured.
    /// </remarks>
    [Parameter] public bool Modal { get; set; } = true;

    private protected override string ModulePath => "./_content/BlazeUI.Headless/js/dialog/dialog.js";
    private protected override string JsInstanceKey => _context.PopupId;

    public DialogRoot()
    {
        _context = new DialogContext
        {
            PopupId = IdGenerator.Next("dialog-popup"),
            // Pre-generate label IDs so they're available in the popup's first render
            // through the Portal. Title and Description use these via ElementId override.
            TitleId = IdGenerator.Next("dialog-title"),
            DescriptionId = IdGenerator.Next("dialog-description"),
        };
        _context.SetOpen = SetOpenAsync;
        _context.Close = () => SetOpenAsync(false);
    }

    private protected override void SyncContextState()
    {
        _context.Open = OpenValue;
        _context.Modal = Modal;
        _context.NestingLevel = (ParentDialogContext?.NestingLevel ?? 0) + 1;
    }

    private protected override Task OnJsInitializedAsync()
    {
        _dotNetRef = DotNetObjectReference.Create(this);
        _context.JsModule = _jsModule;
        _context.DotNetRef = _dotNetRef;

        // The Root owns the JS module and the DefaultOpen parameter, so it is
        // responsible for enqueuing the initial show mutation. Child Popup
        // components only enqueue on subsequent open transitions (when JsModule
        // is guaranteed to be set). FlushAsync runs at the end of the calling
        // OnAfterRenderAsync.
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
    [JSInvokable] public Task OnClickOutside() => SetOpenAsync(false);

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
