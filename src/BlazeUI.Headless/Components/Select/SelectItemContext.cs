namespace BlazeUI.Headless.Components.Select;

/// <summary>
/// Cascaded by <see cref="SelectItem"/> to its children (<see cref="SelectItemIndicator"/>
/// and <see cref="SelectItemText"/>) so they can read item-level state without needing
/// a separate <c>Value</c> parameter.
/// </summary>
internal sealed class SelectItemContext
{
    public bool Selected { get; set; }
    public bool Highlighted { get; set; }
}
