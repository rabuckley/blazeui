using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Combobox;

/// <summary>
/// Button inside a <see cref="ComboboxChip"/> that removes its value from the
/// multiple-selection list. Requires a parent <see cref="ComboboxChip"/>.
/// </summary>
public class ComboboxChipRemove : BlazeElement<ComboboxChipRemoveState>
{
    [CascadingParameter] internal ComboboxContext Context { get; set; } = default!;
    [CascadingParameter] internal ComboboxChipContext ChipContext { get; set; } = default!;

    protected override string DefaultTag => "button";

    private bool EffectivelyDisabled => ChipContext.Disabled || Context.ReadOnly;

    protected override ComboboxChipRemoveState GetCurrentState() =>
        new(EffectivelyDisabled);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-disabled", EffectivelyDisabled ? "" : null);
    }

    private async Task HandleClick()
    {
        if (EffectivelyDisabled) return;
        await Context.RemoveValue(ChipContext.Value);
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

        builder.AddAttribute(6, "type", "button");
        // Not in the standard tab order — keyboard nav handles focus within chips.
        builder.AddAttribute(7, "tabindex", "-1");
        if (EffectivelyDisabled) builder.AddAttribute(8, "disabled", true);

        builder.AddAttribute(9, "onclick",
            EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));

        builder.AddContent(10, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct ComboboxChipRemoveState(bool Disabled);
