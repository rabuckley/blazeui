using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Select;

/// <summary>
/// A container for the select list.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
/// <remarks>
/// This is a plain container — it does not carry <c>role="listbox"</c>.
/// Place a <see cref="SelectList"/> inside for the listbox role, or use
/// <see cref="SelectItem"/> elements directly (they carry <c>role="option"</c>).
/// </remarks>
public class SelectPopup : BlazeElement<SelectPopupState>
{
    [CascadingParameter] internal SelectContext Context { get; set; } = default!;

    private bool _mounted;
    private bool _wasOpen;

    protected override string DefaultTag => "div";
    protected override SelectPopupState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        // data-open/data-closed are owned exclusively by JS (show/hide) to avoid
        // Blazor triggering CSS entry animations before floating-ui positioning.
        yield break;
    }

    protected override void OnParametersSet()
    {
        if (Context.Open && !_wasOpen) _mounted = true;
        _wasOpen = Context.Open;
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

        builder.AddContent(6, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct SelectPopupState(bool Open);
