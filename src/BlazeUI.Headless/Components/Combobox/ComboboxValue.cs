using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Combobox;

/// <summary>
/// Displays the currently selected value's label, or the placeholder when nothing
/// is selected. In multiple-selection mode, displays a comma-separated list of labels.
/// </summary>
public class ComboboxValue : BlazeElement<ComboboxValueState>
{
    [CascadingParameter] internal ComboboxContext Context { get; set; } = default!;

    /// <summary>Overrides the root-level placeholder text for this specific display.</summary>
    [Parameter] public string? Placeholder { get; set; }

    protected override string DefaultTag => "span";

    private bool HasValue =>
        Context.Multiple
            ? Context.SelectedValues.Count > 0
            : Context.SelectedValue is not null;

    protected override ComboboxValueState GetCurrentState() => new(HasValue);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-placeholder", !HasValue ? "" : null);
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

        if (ChildContent is not null)
        {
            // Consumer provides custom rendering.
            builder.AddContent(6, ChildContent);
        }
        else if (Context.Multiple)
        {
            // Multiple selection: show comma-separated labels.
            var labels = Context.SelectedLabels.Count > 0
                ? Context.SelectedLabels
                : Context.SelectedValues;
            var displayText = labels.Count > 0
                ? string.Join(", ", labels)
                : (Placeholder ?? Context.Placeholder);
            builder.AddContent(6, displayText);
        }
        else
        {
            // Single selection: show the selected label, or the placeholder.
            var displayText = Context.SelectedLabel ?? Context.SelectedValue
                ?? Placeholder ?? Context.Placeholder;
            builder.AddContent(6, displayText);
        }

        builder.CloseElement();
    }
}

public readonly record struct ComboboxValueState(bool HasValue);
