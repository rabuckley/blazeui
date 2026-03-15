using BlazeUI.Bridge;
using BlazeUI.Headless.Core;
using BlazeUI.Headless.Overlay.Mutations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Dialog;

/// <summary>
/// Dialog popup rendered as a <c>&lt;div role="dialog"&gt;</c> matching the Base UI
/// reference. Uses the Portal system for top-layer rendering and JS for focus
/// trapping, scroll lock, and animation lifecycle.
/// </summary>
public class DialogPopup : BlazeElement<DialogPopupState>
{
    [CascadingParameter]
    internal DialogContext Context { get; set; } = default!;

    [Inject]
    internal BrowserMutationQueue MutationQueue { get; set; } = default!;

    private bool _mounted;
    private bool _wasOpen;

    protected override string DefaultTag => "div";

    protected override DialogPopupState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        // data-open/data-closed are owned exclusively by JS (show/hide) to avoid
        // Blazor re-renders resetting animation state (opacity flash after close).
        yield break;
    }

    protected override void OnParametersSet()
    {
        if (Context.Open && !_wasOpen)
        {
            _mounted = true;

            // The Root enqueues the initial show mutation for DefaultOpen in
            // OnJsInitializedAsync (when JsModule is guaranteed set). Here we
            // only enqueue on subsequent open transitions.
            if (Context.JsModule is not null)
                MutationQueue.Enqueue(new ShowPopoverMutation
                {
                    ElementId = Context.PopupId,
                    JsModule = Context.JsModule,
                    DotNetRef = Context.DotNetRef!,
                });
        }

        _wasOpen = Context.Open;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await MutationQueue.FlushAsync();
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!_mounted) return;

        // Focus guard before popup — matches Base UI's FloatingFocusManager
        // which traps keyboard focus within the dialog overlay.
        RenderFocusGuard(builder, 0);

        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(10, tag);
        builder.AddAttribute(11, "id", Context.PopupId);

        if (AdditionalAttributes is not null)
            builder.AddMultipleAttributes(12, AdditionalAttributes);

        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass))
            builder.AddAttribute(13, "class", mergedClass);

        var mergedStyle = Css.Cn(Style, StyleBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedStyle))
            builder.AddAttribute(14, "style", mergedStyle);

        foreach (var attr in GetDataAttributes())
            if (attr.Value is not null)
                builder.AddAttribute(15, attr.Key, attr.Value);

        builder.AddAttribute(16, "role", Context.Role);
        builder.AddAttribute(17, "aria-modal", "true");
        builder.AddAttribute(18, "aria-labelledby", Context.TitleId);
        builder.AddAttribute(19, "aria-describedby", Context.DescriptionId);
        // tabIndex="-1" allows focus to land on the popup itself when no inner element
        // is focusable — matches Base UI's FloatingFocusManager behavior.
        builder.AddAttribute(20, "tabindex", "-1");

        builder.AddContent(21, ChildContent);
        builder.CloseElement();

        // Focus guard after popup
        RenderFocusGuard(builder, 30);
    }

    /// <summary>
    /// Renders a visually hidden focus sentinel that traps keyboard focus inside
    /// the dialog, matching Base UI's focus guard spans.
    /// </summary>
    private static void RenderFocusGuard(RenderTreeBuilder builder, int seq)
    {
        builder.OpenElement(seq, "span");
        builder.AddAttribute(seq + 1, "aria-hidden", "true");
        builder.AddAttribute(seq + 2, "tabindex", "0");
        builder.AddAttribute(seq + 3, "style",
            "clip-path: inset(50%); overflow: hidden; white-space: nowrap; " +
            "border: 0px; padding: 0px; width: 1px; height: 1px; margin: -1px; " +
            "position: fixed; top: 0px; left: 0px;");
        builder.CloseElement();
    }
}

public readonly record struct DialogPopupState(bool Open);
