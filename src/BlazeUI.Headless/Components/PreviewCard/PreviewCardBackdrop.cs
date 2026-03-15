using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.PreviewCard;

/// <summary>
/// An overlay displayed beneath the popup. Renders a <c>&lt;div&gt;</c> element
/// with <c>role="presentation"</c>. Hidden while the card is closed and mounted.
/// </summary>
public class PreviewCardBackdrop : BlazeElement<PreviewCardBackdropState>
{
    [CascadingParameter]
    internal PreviewCardContext Context { get; set; } = default!;

    private bool _mounted;
    private bool _wasOpen;

    protected override string DefaultTag => "div";

    protected override PreviewCardBackdropState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-open", Context.Open ? "" : null);
        yield return new("data-closed", !Context.Open ? "" : null);
    }

    protected override void OnParametersSet()
    {
        if (Context.Open && !_wasOpen)
            _mounted = true;
        else if (!Context.Open && !_wasOpen && _mounted)
            _mounted = false;

        _wasOpen = Context.Open;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!_mounted) return;

        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);

        if (AdditionalAttributes is not null)
            builder.AddMultipleAttributes(1, AdditionalAttributes);

        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass))
            builder.AddAttribute(2, "class", mergedClass);

        var mergedStyle = Css.Cn(Style, StyleBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedStyle))
            builder.AddAttribute(3, "style", mergedStyle);

        foreach (var attr in GetDataAttributes())
            if (attr.Value is not null)
                builder.AddAttribute(4, attr.Key, attr.Value);

        // Presentation role; pointer events disabled so interactive popup content is reachable.
        builder.AddAttribute(5, "role", "presentation");

        builder.AddContent(6, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct PreviewCardBackdropState(bool Open);
