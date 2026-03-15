using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Menu;

/// <summary>
/// An accessible label that is automatically associated with its parent
/// <see cref="MenuGroup"/> via <c>aria-labelledby</c>.
/// Renders a <c>&lt;div&gt;</c> element with <c>role="presentation"</c>.
/// </summary>
public class MenuGroupLabel : BlazeElement<MenuGroupLabelState>
{
    [CascadingParameter] internal MenuGroupContext? GroupContext { get; set; }

    protected override string DefaultTag => "div";
    protected override MenuGroupLabelState GetCurrentState() => default;
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes() { yield break; }

    protected override void OnInitialized()
    {
        // Push our resolved ID up to the parent MenuGroup so it can set aria-labelledby.
        // Use OnInitialized (not OnParametersSet) to avoid an infinite re-render loop:
        // SetLabelId calls StateHasChanged on the parent, which re-cascades parameters.
        GroupContext?.SetLabelId(ResolvedId);
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
        // Labels have presentation role — they are referenced via aria-labelledby
        // on the group, so they don't need their own semantic role.
        builder.AddAttribute(5, "role", "presentation");
        builder.AddContent(6, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct MenuGroupLabelState;
