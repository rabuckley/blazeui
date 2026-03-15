using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Combobox;

/// <summary>
/// Clears the combobox input value when clicked.
/// Only visible when there is a value to clear (i.e., the input is non-empty).
/// Renders a <c>&lt;button&gt;</c> element. Matches Base UI's <c>Combobox.Clear</c>.
/// </summary>
public class ComboboxClear : BlazeElement<ComboboxClearState>
{
    [CascadingParameter] internal ComboboxContext Context { get; set; } = default!;

    protected override string DefaultTag => "button";
    protected override ComboboxClearState GetCurrentState() => new(Context.Open, Context.Disabled);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-popup-open", Context.Open ? "" : null);
        yield return new("data-disabled", Context.Disabled ? "" : null);
    }

    private async Task HandleClick()
    {
        if (Context.Disabled) return;

        // Clear the input text and close the popup. Focus returns to the input
        // so the user can immediately type a new query.
        await Context.SetInputValue(null);
        await Context.SetOpen(false);
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
        // tabindex=-1 avoids stealing focus from the input (Base UI does the same).
        builder.AddAttribute(7, "tabindex", "-1");
        builder.AddAttribute(8, "aria-label", "Clear");
        if (Context.Disabled) builder.AddAttribute(9, "disabled", true);

        builder.AddAttribute(10, "onclick",
            EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));
        // Prevent the mousedown from stealing focus from the input — same as Base UI.
        builder.AddEventPreventDefaultAttribute(11, "onmousedown", true);

        builder.AddContent(12, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct ComboboxClearState(bool Open, bool Disabled);
