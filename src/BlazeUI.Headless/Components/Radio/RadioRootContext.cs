namespace BlazeUI.Headless.Components.Radio;

/// <summary>
/// Cascaded from RadioRoot to RadioIndicator, carrying per-radio state.
/// </summary>
internal sealed class RadioRootContext
{
    public bool Checked { get; set; }
    public bool Disabled { get; set; }
    public bool ReadOnly { get; set; }
    public bool Required { get; set; }
}
