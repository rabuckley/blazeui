using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Combobox;

public class ComboboxItem : BlazeElement<ComboboxItemState>
{
    [CascadingParameter] internal ComboboxContext Context { get; set; } = default!;

    [Parameter, EditorRequired] public string Value { get; set; } = "";

    /// <summary>Display text. Falls back to <see cref="Value"/> if not set.</summary>
    [Parameter] public string? Label { get; set; }

    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// When true the item is unconditionally hidden via <c>display:none</c>,
    /// in addition to the built-in filter. Use for consumer-controlled
    /// visibility beyond the default text matching.
    /// </summary>
    [Parameter] public bool Hidden { get; set; }

    // Use IsValueSelected so that both single and multiple selection modes work correctly.
    private bool IsSelected => Context.IsValueSelected(Value);
    private bool IsHighlighted => Context.HighlightedItemId == ResolvedId;

    /// <summary>
    /// The item is filtered out when the user is actively typing and the
    /// label/value doesn't contain the filter text (case-insensitive).
    /// Matches Base UI's default <c>Intl.Collator</c> <c>contains</c> filter
    /// with <c>sensitivity: 'base'</c>.
    /// </summary>
    private bool IsFilteredOut
    {
        get
        {
            if (Hidden) return true;
            var filter = Context.FilterValue;
            if (string.IsNullOrEmpty(filter)) return false;
            var text = Label ?? Value;
            return !text.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }
    }

    // Cascaded to children (e.g. ComboboxItemIndicator) so they can read
    // the parent item's selected state without a redundant Value parameter.
    private readonly ComboboxItemContext _itemContext = new();

    protected override string DefaultTag => "div";

    protected override void OnParametersSet()
    {
        // Register this item's text so the popup can compute data-empty.
        Context.RegisteredItems[ResolvedId] = Label ?? Value;
        // Keep the item context in sync so the indicator re-renders correctly.
        _itemContext.Selected = IsSelected;
    }

    protected override ComboboxItemState GetCurrentState() => new(IsSelected, IsHighlighted, Disabled, IsFilteredOut);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-selected", IsSelected ? "" : null);
        yield return new("data-highlighted", IsHighlighted ? "" : null);
        yield return new("data-disabled", Disabled ? "" : null);
    }

    private async Task HandleClick()
    {
        if (Disabled || Context.ReadOnly) return;
        await Context.SelectItem(Value, Label);
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

        // Merge hidden style with any user-provided style.
        var hiddenStyle = IsFilteredOut ? "display:none;" : null;
        var mergedStyle = Css.Cn(hiddenStyle, Style, StyleBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedStyle)) builder.AddAttribute(4, "style", mergedStyle);

        foreach (var attr in GetDataAttributes())
            if (attr.Value is not null) builder.AddAttribute(5, attr.Key, attr.Value);

        builder.AddAttribute(6, "role", "option");
        builder.AddAttribute(7, "aria-selected", IsSelected ? "true" : "false");
        if (Disabled) builder.AddAttribute(8, "aria-disabled", "true");
        builder.AddAttribute(9, "tabindex", Disabled ? "-1" : "0");

        // Store value and display label as data attributes for JS keyboard nav
        // (inline completion reads data-label to populate the input).
        builder.AddAttribute(10, "data-value", Value);
        builder.AddAttribute(29, "data-label", Label ?? Value);

        builder.AddAttribute(11, "onclick",
            EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));

        // Cascade item context so ComboboxItemIndicator doesn't need a Value parameter.
        builder.OpenComponent<CascadingValue<ComboboxItemContext>>(12);
        builder.AddComponentParameter(13, "Value", _itemContext);
        builder.AddComponentParameter(14, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();
    }
}

public readonly record struct ComboboxItemState(bool Selected, bool Highlighted, bool Disabled, bool Hidden);
