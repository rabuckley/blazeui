using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Tooltip;

/// <summary>
/// The tooltip popup element. Uses a <c>_mounted</c> flag for mount/unmount lifecycle:
/// mounts when the tooltip opens, unmounts after the exit animation completes
/// (signalled by <see cref="TooltipRoot.OnExitAnimationComplete"/>).
/// </summary>
public class TooltipPopup : BlazeElement<TooltipPopupState>
{
    [CascadingParameter]
    internal TooltipContext Context { get; set; } = default!;

    private bool _mounted;
    private bool _wasOpen;

    protected override string DefaultTag => "div";

    protected override TooltipPopupState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        // data-open/data-closed are owned exclusively by JS (positionAndShow/animateAndHide)
        // to avoid Blazor re-renders resetting animation state.
        yield return new("data-side", Context.Side);
        yield return new("data-align", Context.Align);
    }

    protected override void OnParametersSet()
    {
        if (Context.Open && !_wasOpen)
        {
            _mounted = true;
        }
        else if (!Context.Open && !_wasOpen && _mounted)
        {
            // Exit animation completed (driven by TooltipRoot.OnBeforeCloseAsync
            // → OnExitAnimationComplete → StateHasChanged) — unmount the element.
            _mounted = false;
        }

        _wasOpen = Context.Open;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!_mounted) return;

        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddAttribute(1, "id", Context.PopupId);

        if (AdditionalAttributes is not null)
            builder.AddMultipleAttributes(2, AdditionalAttributes);

        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass))
            builder.AddAttribute(3, "class", mergedClass);

        var mergedStyle = Css.Cn(Style, StyleBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedStyle))
            builder.AddAttribute(4, "style", mergedStyle);

        foreach (var attr in GetDataAttributes())
            if (attr.Value is not null)
                builder.AddAttribute(5, attr.Key, attr.Value);

        builder.AddAttribute(6, "role", "tooltip");

        builder.AddContent(7, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct TooltipPopupState(bool Open);
