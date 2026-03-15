using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Combobox;

/// <summary>
/// Button that toggles the combobox popup open/closed.
/// Typically rendered inside a <see cref="ComboboxInputGroup"/> alongside the input.
/// </summary>
public class ComboboxTrigger : BlazeElement<ComboboxTriggerState>
{
    [CascadingParameter] internal ComboboxContext Context { get; set; } = default!;

    protected override string DefaultTag => "button";
    protected override ComboboxTriggerState GetCurrentState() => new(Context.Open, Context.Disabled);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-popup-open", Context.Open ? "" : null);
        yield return new("data-disabled", Context.Disabled ? "" : null);
    }

    private async Task HandleClick()
    {
        await Context.SetOpen(!Context.Open);
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
        // tabindex="-1" keeps focus on the input during keyboard interaction.
        builder.AddAttribute(7, "tabindex", "-1");
        builder.AddAttribute(8, "aria-expanded", Context.Open ? "true" : "false");
        builder.AddAttribute(9, "aria-haspopup", "listbox");
        builder.AddAttribute(10, "aria-controls", Context.ListId);
        if (Context.Disabled) builder.AddAttribute(11, "disabled", true);

        builder.AddAttribute(12, "onclick",
            EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));
        // Prevent the click from bubbling to the input's click handler,
        // which would immediately reopen the popup after the trigger closes it.
        builder.AddEventStopPropagationAttribute(13, "onclick", true);

        builder.AddContent(14, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct ComboboxTriggerState(bool Open, bool Disabled);
