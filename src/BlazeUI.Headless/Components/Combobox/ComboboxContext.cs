using BlazeUI.Headless.Components.Autocomplete;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Combobox;

internal sealed class ComboboxContext
{
    public bool Open { get; set; }

    // -- Single-selection state --

    public string? SelectedValue { get; set; }
    public string? SelectedLabel { get; set; }

    // -- Multiple-selection state --

    /// <summary>True when the combobox is in multiple-selection mode.</summary>
    public bool Multiple { get; set; }

    /// <summary>Selected values in multiple-selection mode.</summary>
    public IReadOnlyList<string> SelectedValues { get; set; } = [];

    /// <summary>Display labels for each selected value in multiple-selection mode.</summary>
    public IReadOnlyList<string> SelectedLabels { get; set; } = [];

    // -- Input state --

    public string? InputValue { get; set; }

    /// <summary>
    /// The active filter text. Only set when the user is actively typing —
    /// cleared on item selection so the full list remains visible after a pick.
    /// Items match against this, not <see cref="InputValue"/>.
    /// </summary>
    public string? FilterValue { get; set; }

    /// <summary>Placeholder text shown by <c>Combobox.Value</c> when nothing is selected.</summary>
    public string? Placeholder { get; set; }

    // -- Focus / highlight state --

    public string? HighlightedItemId { get; set; }

    // -- Element IDs for ARIA wiring --

    public string InputId { get; set; } = "";
    public string PopupId { get; set; } = "";
    public string ListId { get; set; } = "";
    public string PositionerId { get; set; } = "";

    /// <summary>
    /// Set by <see cref="ComboboxLabel"/> so the input can reference it via
    /// <c>aria-labelledby</c>.
    /// </summary>
    public string? LabelId { get; set; }

    // -- Behaviour flags --

    public bool Disabled { get; set; }
    public bool ReadOnly { get; set; }
    public bool Required { get; set; }
    public bool OpenOnFocus { get; set; } = true;

    /// <summary>
    /// When true, keyboard navigation through items temporarily sets the input value
    /// to the highlighted item's display text with the completion portion selected.
    /// Set by <see cref="Autocomplete.AutocompleteRoot"/> for <c>Both</c> and <c>Inline</c> modes.
    /// </summary>
    public bool InlineComplete { get; set; }

    /// <summary>
    /// Set by <see cref="Autocomplete.AutocompleteRoot"/> to communicate the effective
    /// <c>aria-autocomplete</c> mode to <c>ComboboxInput</c>.
    /// Null when the input is part of a Combobox (which always uses <c>list</c>).
    /// </summary>
    public AutocompleteMode? AutocompleteMode { get; set; }

    // -- Delegates back to Root --

    public Func<bool, Task> SetOpen { get; set; } = _ => Task.CompletedTask;
    public Func<Task> Close { get; set; } = () => Task.CompletedTask;
    public Func<string, string?, Task> SelectItem { get; set; } = (_, _) => Task.CompletedTask;
    public Func<string?, Task> SetInputValue { get; set; } = _ => Task.CompletedTask;

    /// <summary>Removes a value from the multiple-selection list.</summary>
    public Func<string, Task> RemoveValue { get; set; } = _ => Task.CompletedTask;

    /// <summary>Clears all selection and resets the input value.</summary>
    public Func<Task> ClearSelection { get; set; } = () => Task.CompletedTask;

    // -- JS interop refs --

    public IJSObjectReference? JsModule { get; set; }
    /// <summary>
    /// Typed as <c>object</c> so that <see cref="AutocompleteRoot"/> (which derives Combobox
    /// behaviour but is a different class) can also store its <c>DotNetObjectReference</c> here.
    /// </summary>
    public object? DotNetRef { get; set; }

    // -- Item registry --

    /// <summary>
    /// Items register their label/value here so the popup can determine
    /// whether the current filter leaves any items visible (for <c>data-empty</c>).
    /// Key = item ID, Value = text to match against (Label ?? Value).
    /// </summary>
    internal Dictionary<string, string> RegisteredItems { get; } = new();

    /// <summary>
    /// Returns true when a non-empty filter is active and no registered
    /// items match it. Used by the popup to set <c>data-empty</c>.
    /// </summary>
    internal bool IsEmpty =>
        !string.IsNullOrEmpty(FilterValue) &&
        RegisteredItems.Values.All(text =>
            !text.Contains(FilterValue, StringComparison.OrdinalIgnoreCase));

    // -- Animation lifecycle --

    /// <summary>
    /// Set to true by <see cref="ComboboxRoot.OnExitAnimationComplete"/> to signal
    /// the Popup that the close animation finished and it can unmount.
    /// </summary>
    public bool ExitAnimationComplete { get; set; }

    // -- Positioning --

    /// <summary>Placement string for floating UI (e.g. "bottom-start"). Set by the Positioner.</summary>
    public string Placement { get; set; } = "bottom-start";

    /// <summary>Offset in pixels from the anchor. Set by the Positioner.</summary>
    public int PlacementOffset { get; set; } = 4;

    // -- Multi-select helpers --

    /// <summary>
    /// Returns true when the given value is among the selected values.
    /// In single-selection mode, falls back to <see cref="SelectedValue"/> equality.
    /// </summary>
    internal bool IsValueSelected(string value) =>
        Multiple
            ? SelectedValues.Contains(value)
            : SelectedValue == value;
}
