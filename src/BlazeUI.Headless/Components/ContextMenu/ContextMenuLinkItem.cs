using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.ContextMenu;

/// <summary>
/// A context menu item that renders as an anchor element.
/// </summary>
public class ContextMenuLinkItem : BlazeElement<ContextMenuLinkItemState>
{
    [CascadingParameter] internal ContextMenuContext Context { get; set; } = default!;

    [Parameter] public bool Disabled { get; set; }

    private bool IsHighlighted => Context.HighlightedItemId == ResolvedId;

    protected override string DefaultTag => "a";
    protected override ContextMenuLinkItemState GetCurrentState() => new(IsHighlighted, Disabled);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-highlighted", IsHighlighted ? "" : null);
        yield return new("data-disabled", Disabled ? "" : null);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddAttribute(1, "id", ResolvedId);
        if (AdditionalAttributes is not null) builder.AddMultipleAttributes(2, AdditionalAttributes);
        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass)) builder.AddAttribute(3, "class", mergedClass);
        var mergedStyle = Css.Cn(Style, StyleBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedStyle)) builder.AddAttribute(4, "style", mergedStyle);
        foreach (var attr in GetDataAttributes())
            if (attr.Value is not null) builder.AddAttribute(5, attr.Key, attr.Value);

        builder.AddAttribute(6, "role", "menuitem");
        builder.AddAttribute(7, "tabindex", Disabled ? "-1" : "0");
        if (Disabled) builder.AddAttribute(8, "aria-disabled", "true");

        builder.AddContent(9, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct ContextMenuLinkItemState(bool Highlighted, bool Disabled);
