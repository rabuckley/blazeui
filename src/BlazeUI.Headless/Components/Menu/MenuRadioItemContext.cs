namespace BlazeUI.Headless.Components.Menu;

/// <summary>
/// Internal context cascaded by <see cref="MenuRadioItem"/> to its children
/// so that <see cref="MenuRadioItemIndicator"/> can read the item's checked and
/// disabled state without needing its own parameters.
/// </summary>
internal sealed class MenuRadioItemContext
{
    public bool Checked { get; set; }
    public bool Disabled { get; set; }
}
