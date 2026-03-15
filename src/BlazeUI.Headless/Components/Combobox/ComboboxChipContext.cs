namespace BlazeUI.Headless.Components.Combobox;

/// <summary>
/// Cascaded by <see cref="ComboboxChip"/> so that <see cref="ComboboxChipRemove"/>
/// can identify which value to remove when clicked.
/// </summary>
internal sealed class ComboboxChipContext
{
    public string Value { get; set; } = "";
    public bool Disabled { get; set; }
}
