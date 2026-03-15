using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Select;

/// <summary>
/// A visual separator between select items or groups.
/// Mirrors Base UI's re-export of the shared <c>Separator</c> component within the Select namespace.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
public class SelectSeparator : BlazeElement<SelectSeparatorState>
{
    protected override string DefaultTag => "div";
    protected override SelectSeparatorState GetCurrentState() => default;
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes() => [];

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
        builder.AddAttribute(5, "role", "separator");
        builder.AddAttribute(6, "aria-orientation", "horizontal");
        builder.AddContent(7, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct SelectSeparatorState;
