namespace BlazeUI.Headless.Components.CheckboxGroup;

/// <summary>
/// Cascading context shared between <see cref="CheckboxGroupRoot"/> and the
/// <c>CheckboxRoot</c> components nested within it. Child checkboxes read this
/// to determine their checked state and delegate toggle calls back to the group.
/// </summary>
internal sealed class CheckboxGroupContext
{
    /// <summary>
    /// Returns whether the given value is in the group's current selection.
    /// </summary>
    public Func<string, bool> IsChecked { get; set; } = _ => false;

    /// <summary>
    /// Adds or removes the given value from the group's selection.
    /// </summary>
    public Func<string, Task> Toggle { get; set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Whether all checkboxes in the group are disabled. When <c>true</c>, this
    /// takes precedence over individual checkbox <c>Disabled</c> props.
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// All possible values in the group. Required for parent checkbox support —
    /// a checkbox with <c>parent=true</c> uses this list to determine its
    /// checked/indeterminate state and to select or deselect all children.
    /// </summary>
    public IReadOnlyList<string>? AllValues { get; set; }
}
