namespace BlazeUI.Headless.Components.Combobox;

/// <summary>
/// Cascaded by <see cref="ComboboxItem"/> to its children so that
/// <see cref="ComboboxItemIndicator"/> can read the parent item's selected state
/// without requiring a redundant <c>Value</c> parameter.
/// </summary>
internal sealed class ComboboxItemContext
{
    public bool Selected { get; set; }
}
