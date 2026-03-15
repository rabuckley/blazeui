using BlazeUI.Bridge;
using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Combobox;

/// <summary>
/// Listbox popup for the combobox. Enqueues <see cref="ShowComboboxMutation"/> /
/// <see cref="HideComboboxMutation"/> onto <see cref="BrowserMutationQueue"/> following
/// the same lifecycle pattern as DialogPopup. The Positioner element (parent) is
/// positioned by floating-ui and promoted to the top layer; this Popup handles
/// data attributes and animation classes only.
/// </summary>
public class ComboboxPopup : BlazeElement<ComboboxPopupState>
{
    [CascadingParameter] internal ComboboxContext Context { get; set; } = default!;

    [Inject] internal BrowserMutationQueue MutationQueue { get; set; } = default!;

    private bool _mounted;
    private bool _wasOpen;


    protected override string DefaultTag => "div";
    protected override ComboboxPopupState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        // data-open and data-closed are both JS-owned. JS show() sets data-open
        // AFTER floating-ui positions the popup (prevents slide-from-origin on
        // first open). JS hide() sets data-closed AND attaches the animationend
        // listener in the same synchronous block (prevents the listener missing
        // the event if Blazor's render triggers the animation first).
        yield return new("data-empty", Context.IsEmpty ? "" : null);
        // Propagate placement side from the Positioner so slide-in-from-*
        // animation classes activate (e.g. data-[side=bottom]:slide-in-from-top-2).
        yield return new("data-side", PlacementHelper.ToDataSide(Context.Placement));
    }

    private ShowComboboxMutation CreateShowMutation() => new()
    {
        ElementId = Context.PopupId,
        PositionerId = Context.PositionerId,
        InputId = Context.InputId,
        JsModule = Context.JsModule!,
        DotNetRef = Context.DotNetRef!,
        Placement = Context.Placement,
        Offset = Context.PlacementOffset,
        InlineComplete = Context.InlineComplete,
    };

    protected override void OnParametersSet()
    {
        if (Context.Open && !_wasOpen)
        {
            _mounted = true;
            Context.ExitAnimationComplete = false;

            if (Context.JsModule is not null)
                MutationQueue.Enqueue(CreateShowMutation());
        }
        else if (Context.ExitAnimationComplete)
        {
            // OnExitAnimationComplete signalled via the context that the
            // exit animation finished. Unmount the popup (matches the
            // reference which removes the popup from the DOM on close).
            _mounted = false;
            Context.ExitAnimationComplete = false;
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

        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddAttribute(1, "id", Context.PopupId);
        if (AdditionalAttributes is not null) builder.AddMultipleAttributes(2, AdditionalAttributes);
        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass)) builder.AddAttribute(3, "class", mergedClass);
        var mergedStyle = Css.Cn(Style, StyleBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedStyle)) builder.AddAttribute(4, "style", mergedStyle);
        foreach (var attr in GetDataAttributes())
            if (attr.Value is not null) builder.AddAttribute(5, attr.Key, attr.Value);

        builder.AddAttribute(6, "role", "presentation");

        builder.AddContent(9, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct ComboboxPopupState(bool Open);
