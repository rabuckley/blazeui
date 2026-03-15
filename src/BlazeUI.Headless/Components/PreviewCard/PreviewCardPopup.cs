using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.PreviewCard;

/// <summary>
/// Preview card popup container. Unlike <c>Tooltip</c>, does NOT use
/// <c>role="tooltip"</c> because the popup can contain interactive content.
/// </summary>
public class PreviewCardPopup : BlazeElement<PreviewCardPopupState>
{
    [CascadingParameter]
    internal PreviewCardContext Context { get; set; } = default!;

    private bool _mounted;
    private bool _wasOpen;

    protected override string DefaultTag => "div";

    protected override PreviewCardPopupState GetCurrentState() =>
        new(Context.Open, Context.Side, Context.Align);

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
            // Exit animation completed — notified by PreviewCardRoot.OnExitAnimationComplete
            // → StateHasChanged which re-runs OnParametersSet with Open=false and _wasOpen=false.
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

        builder.AddAttribute(6, "aria-labelledby", Context.TitleId);
        builder.AddAttribute(7, "aria-describedby", Context.DescriptionId);

        builder.AddContent(8, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct PreviewCardPopupState(bool Open, string Side, string Align);
