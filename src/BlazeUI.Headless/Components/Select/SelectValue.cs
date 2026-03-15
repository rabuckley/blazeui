using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Select;

/// <summary>
/// Displays the currently selected value's label, or the placeholder if nothing is selected.
/// </summary>
public class SelectValue : BlazeElement<SelectValueState>
{
    [CascadingParameter] internal SelectContext Context { get; set; } = default!;

    protected override string DefaultTag => "span";
    protected override SelectValueState GetCurrentState() => new(Context.SelectedValue is not null);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-placeholder", Context.SelectedValue is null ? "" : null);
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

        // Show selected label, fallback to selected value, fallback to placeholder or ChildContent
        var displayText = Context.SelectedLabel ?? Context.SelectedValue;
        if (displayText is not null)
        {
            builder.AddContent(6, displayText);
        }
        else if (ChildContent is not null)
        {
            builder.AddContent(6, ChildContent);
        }
        else
        {
            builder.AddContent(6, Context.Placeholder);
        }

        builder.CloseElement();
    }
}

public readonly record struct SelectValueState(bool HasValue);
