using BlazeUI.Bridge;
using BlazeUI.Headless.Core;
using BlazeUI.Headless.Overlay.Mutations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Drawer;

/// <summary>
/// A container for the drawer contents.
/// Renders a <c>&lt;div&gt;</c> element with <c>role="dialog"</c>.
/// </summary>
public class DrawerPopup : BlazeElement<DrawerPopupState>
{
    [CascadingParameter] internal DrawerContext Context { get; set; } = default!;

    [Inject]
    internal BrowserMutationQueue MutationQueue { get; set; } = default!;

    private bool _mounted;
    private bool _wasOpen;

    protected override string DefaultTag => "div";

    protected override DrawerPopupState GetCurrentState() =>
        new(Context.Open, Context.SwipeDirection.ToString().ToLowerInvariant());

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        // data-open/data-closed are owned exclusively by JS (show/hide) to avoid
        // Blazor re-renders resetting animation state (opacity flash after close).
        // data-swipe-direction indicates which direction swipes the drawer away.
        yield return new("data-swipe-direction", Context.SwipeDirection.ToString().ToLowerInvariant());
        // data-side is the anchor edge (opposite of swipe direction). Used by styled
        // templates for direction-specific classes (e.g. drag handle, text alignment).
        yield return new("data-side", GetAnchorSide());
        // data-swiping is set/removed by JS during active drag-to-dismiss tracking.
        // TODO: data-expanded, data-nested-drawer-open, data-nested-drawer-swiping
        // require JS-side measurement (popup height, nested drawer state). These are deferred
        // until BlazeUI has a measurement/nested-drawer-tracking infrastructure equivalent
        // to Base UI's DrawerRootContext state stores.
    }

    private string GetAnchorSide() => Context.SwipeDirection switch
    {
        SwipeDirection.Up => "top",
        SwipeDirection.Down => "bottom",
        SwipeDirection.Left => "left",
        SwipeDirection.Right => "right",
        _ => "bottom"
    };

    protected override void OnParametersSet()
    {
        if (Context.Open && !_wasOpen)
        {
            _mounted = true;

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

        builder.AddAttribute(6, "role", "dialog");
        builder.AddAttribute(7, "tabindex", "-1");
        builder.AddAttribute(8, "aria-modal", "true");
        builder.AddAttribute(9, "aria-labelledby", Context.TitleId);
        builder.AddAttribute(10, "aria-describedby", Context.DescriptionId);

        builder.AddContent(11, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct DrawerPopupState(bool Open, string SwipeDirection);
