using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Menu;

/// <summary>
/// A separator element accessible to screen readers.
/// Renders a <c>&lt;div&gt;</c> element with <c>role="separator"</c>.
/// </summary>
public class MenuSeparator : BlazeElement<MenuSeparatorState>
{
    /// <summary>The orientation of the separator. Defaults to <c>horizontal</c>.</summary>
    [Parameter] public Orientation Orientation { get; set; } = Orientation.Horizontal;

    protected override string DefaultTag => "div";
    protected override MenuSeparatorState GetCurrentState() => new(Orientation);
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes() => [];

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddAttribute(1, "id", ResolvedId);

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

        builder.AddAttribute(6, "role", "separator");
        builder.AddAttribute(7, "aria-orientation", Orientation == Orientation.Vertical ? "vertical" : "horizontal");

        builder.AddContent(8, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct MenuSeparatorState(Orientation Orientation);
