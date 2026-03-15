namespace BlazeUI.Headless.Components.NumberField;

/// <summary>
/// Context cascaded by <see cref="NumberFieldScrubArea"/> to its children.
/// Provides scrub state for the <see cref="NumberFieldScrubAreaCursor"/>.
/// </summary>
internal sealed class NumberFieldScrubAreaContext
{
    public bool IsScrubbing { get; set; }
    public string Direction { get; set; } = "horizontal";
    public string ScrubAreaId { get; set; } = "";
}
