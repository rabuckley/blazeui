namespace BlazeUI.Headless.Components.Field;

/// <summary>
/// Cascaded context from <see cref="FieldItem"/> to the checkbox or radio controls nested
/// within it. Carries the per-item disabled state so controls can disable independently
/// from the parent field.
/// </summary>
internal sealed class FieldItemContext
{
    /// <summary>
    /// Whether controls within this item are disabled. Reflects the combined disabled state
    /// from both the <see cref="FieldItem.Disabled"/> prop and the parent
    /// <see cref="FieldRoot.Disabled"/> prop.
    /// </summary>
    public bool Disabled { get; set; }
}
