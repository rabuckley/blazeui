using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.ContextMenu;

/// <summary>
/// The popup element for the context menu. Renders a <c>role="menu"</c> element
/// inside the positioner. Positioning is handled by <see cref="ContextMenuPositioner"/>;
/// this component only renders the visual container and menu items.
/// </summary>
public class ContextMenuPopup : BlazeElement<ContextMenuPopupState>
{
    [CascadingParameter] internal ContextMenuContext Context { get; set; } = default!;

    private bool _mounted;
    private bool _wasOpen;

    protected override string DefaultTag => "div";
    protected override ContextMenuPopupState GetCurrentState() => new(Context.Open);

    // data-open/data-closed are JS-owned for overlay popups. Blazor must NOT emit
    // them — JS sets data-open after positioning and data-closed synchronously on
    // close to drive CSS animations without race conditions.
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        return [];
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

        builder.AddAttribute(5, "role", "menu");
        builder.AddAttribute(6, "tabindex", "-1");

        builder.AddContent(7, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct ContextMenuPopupState(bool Open);
