using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Meter;

/// <summary>
/// A text element displaying the current value.
/// Renders a <c>&lt;span&gt;</c> element. When no child content is provided,
/// renders the pre-formatted value from <see cref="MeterRoot"/> (percentage by
/// default, or formatted with the root's <c>Format</c> options when set).
/// The element is hidden from the accessibility tree via <c>aria-hidden="true"</c>
/// because the meter value is already announced via <c>aria-valuenow</c>/<c>aria-valuetext</c>
/// on the root.
/// </summary>
public class MeterValue : BlazeElement<MeterValueState>
{
    [CascadingParameter] internal MeterContext Context { get; set; } = default!;

    protected override string DefaultTag => "span";

    protected override MeterValueState GetCurrentState() =>
        new(Context.Value, Context.Min, Context.Max, Context.FormattedValue);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield break;
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        // Hidden from AT — the root's aria-valuenow/aria-valuetext already conveys the value.
        yield return new("aria-hidden", "true");
    }
}

/// <summary>State for <see cref="MeterValue"/>, exposed via <c>ClassBuilder</c>/<c>StyleBuilder</c>.</summary>
public readonly record struct MeterValueState(
    double Value,
    double Min,
    double Max,
    string FormattedValue);
