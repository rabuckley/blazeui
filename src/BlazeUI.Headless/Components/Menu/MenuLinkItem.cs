using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Menu;

/// <summary>
/// A link in the menu that can be used to navigate to a different page or section.
/// Renders an <c>&lt;a&gt;</c> element.
/// </summary>
public class MenuLinkItem : BlazeElement<MenuLinkItemState>
{
    [CascadingParameter] internal MenuContext Context { get; set; } = default!;

    private bool IsHighlighted => Context.HighlightedItemId == ResolvedId;

    protected override string DefaultTag => "a";
    protected override MenuLinkItemState GetCurrentState() => new(IsHighlighted);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        // Base UI's MenuLinkItemDataAttributes only defines data-highlighted.
        yield return new("data-highlighted", IsHighlighted ? "" : null);
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
        builder.AddAttribute(7, "tabindex", "0");

        builder.AddContent(8, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct MenuLinkItemState(bool Highlighted);
