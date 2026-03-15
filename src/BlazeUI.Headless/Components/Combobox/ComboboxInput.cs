using BlazeUI.Headless.Components.Autocomplete;
using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Combobox;

/// <summary>
/// Text input for filtering and selecting items. Renders an <c>&lt;input&gt;</c> element
/// with <c>role="combobox"</c> and the required ARIA attributes wired to the listbox.
/// </summary>
public class ComboboxInput : BlazeElement<ComboboxInputState>
{
    [CascadingParameter] internal ComboboxContext Context { get; set; } = default!;

    [Parameter] public string? Placeholder { get; set; }

    protected override string DefaultTag => "input";
    protected override ComboboxInputState GetCurrentState() => new(Context.Open, Context.Disabled, Context.ReadOnly, Context.Required);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-popup-open", Context.Open ? "" : null);
        yield return new("data-disabled", Context.Disabled ? "" : null);
        yield return new("data-readonly", Context.ReadOnly ? "" : null);
    }

    private Task HandleInput(ChangeEventArgs e)
    {
        return Context.SetInputValue(e.Value?.ToString());
    }

    private Task HandleFocus()
    {
        if (Context.OpenOnFocus && !Context.Open)
            return Context.SetOpen(true);
        return Task.CompletedTask;
    }

    // Reopens the popup when clicking the already-focused input. onfocus
    // alone is insufficient because clicking an already-focused input
    // doesn't re-fire the focus event.
    private Task HandleClick()
    {
        if (Context.OpenOnFocus && !Context.Open)
            return Context.SetOpen(true);
        return Task.CompletedTask;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddAttribute(1, "id", Context.InputId);
        if (AdditionalAttributes is not null) builder.AddMultipleAttributes(2, AdditionalAttributes);
        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass)) builder.AddAttribute(3, "class", mergedClass);
        var mergedStyle = Css.Cn(Style, StyleBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedStyle)) builder.AddAttribute(4, "style", mergedStyle);
        foreach (var attr in GetDataAttributes())
            if (attr.Value is not null) builder.AddAttribute(5, attr.Key, attr.Value);

        builder.AddAttribute(6, "role", "combobox");
        builder.AddAttribute(7, "aria-expanded", Context.Open ? "true" : "false");
        builder.AddAttribute(8, "aria-controls", Context.ListId);
        // Derive aria-autocomplete from the Autocomplete mode when set; default to "list"
        // for Combobox usage where mode is not specified.
        var ariaAutocomplete = Context.AutocompleteMode switch
        {
            AutocompleteMode.Both or AutocompleteMode.Inline => "both",
            AutocompleteMode.None => "none",
            _ => "list",
        };
        builder.AddAttribute(9, "aria-autocomplete", ariaAutocomplete);
        if (Context.LabelId is not null)
            builder.AddAttribute(10, "aria-labelledby", Context.LabelId);
        if (Context.HighlightedItemId is not null)
            builder.AddAttribute(11, "aria-activedescendant", Context.HighlightedItemId);
        if (Context.ReadOnly) builder.AddAttribute(12, "aria-readonly", "true");
        if (Context.Required) builder.AddAttribute(13, "aria-required", "true");
        builder.AddAttribute(14, "value", Context.InputValue ?? "");
        if (Context.Disabled) builder.AddAttribute(15, "disabled", true);
        // Use explicit Placeholder parameter in preference to context placeholder.
        var placeholder = Placeholder ?? Context.Placeholder;
        if (placeholder is not null) builder.AddAttribute(16, "placeholder", placeholder);
        builder.AddAttribute(17, "autocomplete", "off");

        builder.AddAttribute(18, "oninput",
            EventCallback.Factory.Create<ChangeEventArgs>(this, HandleInput));
        builder.AddAttribute(19, "onfocus",
            EventCallback.Factory.Create<FocusEventArgs>(this, HandleFocus));
        builder.AddAttribute(20, "onclick",
            EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));

        builder.AddContent(21, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct ComboboxInputState(bool Open, bool Disabled, bool ReadOnly, bool Required);
