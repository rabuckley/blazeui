using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Combobox;

/// <summary>
/// Container that renders selected-value chips for a multiple-selection combobox.
/// Applies <c>role="toolbar"</c> when chips are present so screen readers keep
/// focus mode active during arrow-key navigation (NVDA compatibility).
/// </summary>
public class ComboboxChips : BlazeElement<ComboboxChipsState>
{
    [CascadingParameter] internal ComboboxContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";

    private bool HasChips => Context.Multiple && Context.SelectedValues.Count > 0;

    protected override ComboboxChipsState GetCurrentState() => new(HasChips);

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

        // NVDA enters browse mode inside a container with arrow key navigation unless
        // the container carries role="toolbar". Apply it only when chips are present.
        if (HasChips)
            builder.AddAttribute(5, "role", "toolbar");

        builder.AddContent(6, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct ComboboxChipsState(bool HasChips);
