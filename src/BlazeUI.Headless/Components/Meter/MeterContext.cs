namespace BlazeUI.Headless.Components.Meter;

/// <summary>
/// Cascaded context from <see cref="MeterRoot"/> to all Meter sub-parts.
/// Carries the numeric value, range bounds, and the pre-formatted display string
/// so children can render without re-computing formatting.
/// </summary>
internal sealed class MeterContext
{
    // Numeric value and range, used by MeterIndicator to compute its width.
    public double Value { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }

    // Pre-formatted value string for MeterValue to display.
    public string FormattedValue { get; set; } = "";

    // The label's ID — written by MeterLabel during initialization so the
    // root can emit aria-labelledby without knowing the label's ID up front.
    public string? LabelId { get; private set; }

    private readonly Func<Task>? _onChanged;

    public MeterContext(Func<Task>? onChanged = null)
    {
        _onChanged = onChanged;
    }

    /// <summary>
    /// Called by <see cref="MeterLabel"/> when it initializes to register its element ID.
    /// Triggers a re-render of <see cref="MeterRoot"/> so <c>aria-labelledby</c> is updated.
    /// </summary>
    public void SetLabelId(string id)
    {
        LabelId = id;
        _onChanged?.Invoke();
    }
}
