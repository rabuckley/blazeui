using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Select;

/// <summary>
/// An individual option in the select popup.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
public class SelectItem : BlazeElement<SelectItemState>
{
    [CascadingParameter] internal SelectContext Context { get; set; } = default!;

    [Parameter, EditorRequired] public string Value { get; set; } = "";

    /// <summary>Display text. Falls back to trimmed ChildContent text if not set.</summary>
    [Parameter] public string? Label { get; set; }

    [Parameter] public bool Disabled { get; set; }

    private bool IsSelected => Context.SelectedValue == Value;
    private bool IsHighlighted => Context.HighlightedItemId == ResolvedId;

    // The SelectItemContext lets nested SelectItemIndicator/SelectItemText read
    // item-level state without requiring a redundant Value parameter.
    private readonly SelectItemContext _itemContext = new();

    protected override string DefaultTag => "div";
    protected override SelectItemState GetCurrentState() => new(IsSelected, IsHighlighted, Disabled);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-selected", IsSelected ? "" : null);
        yield return new("data-highlighted", IsHighlighted ? "" : null);
        yield return new("data-disabled", Disabled ? "" : null);
    }

    private async Task HandleClick()
    {
        if (Disabled) return;
        var label = Label;

        // When no explicit Label is set, read the rendered text content from
        // the DOM as a fallback — matches Base UI's ItemText label extraction.
        if (label is null && Context.JsModule is not null)
            label = await Context.JsModule.InvokeAsync<string?>("getItemLabel", ResolvedId);

        await Context.SelectItem(Value, label);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        // Update item context so nested children reflect current state.
        _itemContext.Selected = IsSelected;
        _itemContext.Highlighted = IsHighlighted;

        builder.OpenComponent<CascadingValue<SelectItemContext>>(0);
        builder.AddComponentParameter(1, "Value", _itemContext);
        builder.AddComponentParameter(2, "IsFixed", true);
        builder.AddComponentParameter(3, "ChildContent", (RenderFragment)(inner =>
        {
            inner.OpenElement(0, tag);
            inner.AddAttribute(1, "id", ResolvedId);
            if (AdditionalAttributes is not null) inner.AddMultipleAttributes(2, AdditionalAttributes);
            var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
            if (!string.IsNullOrEmpty(mergedClass)) inner.AddAttribute(3, "class", mergedClass);
            var mergedStyle = Css.Cn(Style, StyleBuilder?.Invoke(state));
            if (!string.IsNullOrEmpty(mergedStyle)) inner.AddAttribute(4, "style", mergedStyle);
            foreach (var attr in GetDataAttributes())
                if (attr.Value is not null) inner.AddAttribute(5, attr.Key, attr.Value);

            inner.AddAttribute(6, "role", "option");
            inner.AddAttribute(7, "aria-selected", IsSelected ? "true" : "false");
            if (Disabled) inner.AddAttribute(8, "aria-disabled", "true");
            inner.AddAttribute(9, "tabindex", Disabled ? "-1" : "0");

            // Store value as data attribute for JS keyboard nav to read.
            inner.AddAttribute(10, "data-value", Value);

            inner.AddAttribute(11, "onclick",
                EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));

            inner.AddContent(12, ChildContent);
            inner.CloseElement();
        }));
        builder.CloseComponent();
    }
}

public readonly record struct SelectItemState(bool Selected, bool Highlighted, bool Disabled);
