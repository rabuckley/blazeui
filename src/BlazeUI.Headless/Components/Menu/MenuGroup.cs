using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Menu;

/// <summary>
/// Groups related menu items with the corresponding label.
/// Renders a <c>&lt;div&gt;</c> element with <c>role="group"</c>.
/// When a <see cref="MenuGroupLabel"/> is nested inside, the group's
/// <c>aria-labelledby</c> is automatically wired to the label's ID.
/// </summary>
public class MenuGroup : BlazeElement<MenuGroupState>
{
    // Cascaded inward to the child MenuGroupLabel.
    private readonly MenuGroupContext _groupContext;

    public MenuGroup()
    {
        // The lambda captures the field after construction completes, so _groupContext
        // is non-null when SetLabelId is ever invoked.
        var ctx = new MenuGroupContext();
        ctx.SetLabelId = id =>
        {
            ctx.LabelId = id;
            // Trigger re-render so aria-labelledby picks up the new id.
            StateHasChanged();
        };
        _groupContext = ctx;
    }

    protected override string DefaultTag => "div";
    protected override MenuGroupState GetCurrentState() => default;
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes() { yield break; }

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
        builder.AddAttribute(5, "role", "group");
        if (_groupContext.LabelId is not null)
            builder.AddAttribute(6, "aria-labelledby", _groupContext.LabelId);

        // Cascade MenuGroupContext so MenuGroupLabel can register its ID.
        builder.OpenComponent<CascadingValue<MenuGroupContext>>(7);
        builder.AddComponentParameter(8, "Value", _groupContext);
        builder.AddComponentParameter(9, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();
    }
}

public readonly record struct MenuGroupState;
