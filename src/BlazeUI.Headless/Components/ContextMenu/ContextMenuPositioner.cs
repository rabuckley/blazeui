using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.ContextMenu;

/// <summary>
/// Wraps the popup with a positioned container. JS positions this element at cursor
/// coordinates via <c>showAtPosition</c>; the popup renders inside it with visual styling only.
/// Mirrors Base UI's <c>ContextMenu.Positioner</c>.
/// </summary>
public class ContextMenuPositioner : BlazeElement<ContextMenuPositionerState>
{
    [CascadingParameter] internal ContextMenuContext Context { get; set; } = default!;

    private bool _mounted;
    private bool _wasOpen;

    protected override string DefaultTag => "div";
    protected override ContextMenuPositionerState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        // data-side and data-align are always "right"/"start" for context menus
        // (cursor-anchored, no anchor-relative positioning).
        yield return new("data-side", "right");
        yield return new("data-align", "start");
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
        builder.AddAttribute(1, "id", Context.PositionerId);
        if (AdditionalAttributes is not null) builder.AddMultipleAttributes(2, AdditionalAttributes);
        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass)) builder.AddAttribute(3, "class", mergedClass);
        // position: fixed is set by JS when positioning; include it in initial style
        // so the element doesn't flash at its static position.
        builder.AddAttribute(4, "style", Css.Cn("position: fixed;", Style, StyleBuilder?.Invoke(state)));
        foreach (var attr in GetDataAttributes())
            if (attr.Value is not null) builder.AddAttribute(5, attr.Key, attr.Value);
        builder.AddAttribute(6, "role", "presentation");
        builder.AddContent(7, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct ContextMenuPositionerState(bool Open);
