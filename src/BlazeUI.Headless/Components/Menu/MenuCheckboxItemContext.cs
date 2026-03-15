namespace BlazeUI.Headless.Components.Menu;

/// <summary>
/// Internal context cascaded by <see cref="MenuCheckboxItem"/> to its children
/// so that <see cref="MenuCheckboxItemIndicator"/> can read the item's checked and
/// disabled state without needing its own parameters.
/// </summary>
internal sealed class MenuCheckboxItemContext
{
    public bool Checked { get; set; }
    public bool Disabled { get; set; }
}
