namespace BlazeUI.Headless.Components.Combobox;

/// <summary>
/// Cascaded by <see cref="ComboboxGroup"/> to its children so that
/// <see cref="ComboboxGroupLabel"/> can register its auto-generated ID
/// and the Group can wire <c>aria-labelledby</c> after the label mounts.
/// </summary>
internal sealed class ComboboxGroupContext
{
    /// <summary>
    /// The ID registered by the <see cref="ComboboxGroupLabel"/> inside this group.
    /// Null until the label has rendered.
    /// </summary>
    public string? LabelId { get; set; }

    /// <summary>
    /// Called by <see cref="ComboboxGroupLabel"/> when it mounts so the parent
    /// Group can trigger a re-render with the correct <c>aria-labelledby</c> value.
    /// </summary>
    public Action? OnLabelRegistered { get; set; }
}
