using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Combobox;

/// <summary>
/// A wrapper for the combobox input and its associated controls (trigger button, clear button, icon).
/// Renders a <c>&lt;div role="group"&gt;</c> element. Matches Base UI's <c>Combobox.InputGroup</c>.
/// </summary>
public class ComboboxInputGroup : BlazeElement<ComboboxInputGroupState>
{
    [CascadingParameter] internal ComboboxContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";
    protected override ComboboxInputGroupState GetCurrentState() => new(Context.Open, Context.Disabled);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-popup-open", Context.Open ? "" : null);
        yield return new("data-disabled", Context.Disabled ? "" : null);
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
        builder.AddAttribute(6, "role", "group");
        builder.AddAttribute(7, "data-slot", "input-group");
        builder.AddContent(8, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct ComboboxInputGroupState(bool Open, bool Disabled);
