using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Combobox;

/// <summary>
/// A visual chip representing a single selected value in a multiple-selection combobox.
/// Cascades a <see cref="ComboboxChipContext"/> so the nested
/// <see cref="ComboboxChipRemove"/> button knows which value to remove.
/// </summary>
public class ComboboxChip : BlazeElement<ComboboxChipState>
{
    [CascadingParameter] internal ComboboxContext Context { get; set; } = default!;

    /// <summary>The selected value this chip represents.</summary>
    [Parameter, EditorRequired] public string Value { get; set; } = "";

    [Parameter] public bool Disabled { get; set; }

    private readonly ComboboxChipContext _chipContext = new();

    protected override string DefaultTag => "div";

    protected override void OnParametersSet()
    {
        // Keep the chip context in sync so ChipRemove targets the correct value.
        _chipContext.Value = Value;
        _chipContext.Disabled = Disabled || Context.Disabled;
    }

    protected override ComboboxChipState GetCurrentState() =>
        new(Disabled || Context.Disabled, Context.ReadOnly);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-disabled", (Disabled || Context.Disabled) ? "" : null);
        yield return new("data-readonly", Context.ReadOnly ? "" : null);
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

        if (state.Disabled) builder.AddAttribute(6, "aria-disabled", "true");
        if (state.ReadOnly) builder.AddAttribute(7, "aria-readonly", "true");
        // Chips are not in the tab order; focus management is handled by the Chips container.
        builder.AddAttribute(8, "tabindex", "-1");

        // Cascade context so ComboboxChipRemove can identify the target value.
        builder.OpenComponent<CascadingValue<ComboboxChipContext>>(9);
        builder.AddComponentParameter(10, "Value", _chipContext);
        builder.AddComponentParameter(11, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();
    }
}

public readonly record struct ComboboxChipState(bool Disabled, bool ReadOnly);
