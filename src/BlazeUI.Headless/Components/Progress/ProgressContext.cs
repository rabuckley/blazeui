namespace BlazeUI.Headless.Components.Progress;

internal sealed class ProgressContext
{
    public double? Value { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public ProgressStatus Status { get; set; }

    // Formatted value for display (e.g. "30%" or "$30.00"), empty when indeterminate.
    public string FormattedValue { get; set; } = "";

    // Set by ProgressLabel.OnInitialized so the Root can emit aria-labelledby.
    public string? LabelId { get; set; }

    // Called by ProgressLabel to notify the Root that a label has been registered,
    // triggering a re-render so aria-labelledby can be emitted.
    public Action<string>? SetLabelId { get; set; }

    /// <summary>
    /// The value as a 0–100 percentage, or null when indeterminate.
    /// </summary>
    public double? Percentage => Value.HasValue
        ? (Value.Value - Min) / (Max - Min) * 100
        : null;
}
