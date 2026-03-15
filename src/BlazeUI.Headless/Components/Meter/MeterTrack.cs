using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Meter;

/// <summary>
/// Contains the meter indicator and represents the entire range of the meter.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
public class MeterTrack : BlazeElement<MeterState>
{
    [CascadingParameter]
    internal MeterContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";

    protected override MeterState GetCurrentState() => new(Context.Value, Context.Min, Context.Max);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield break;
    }
}
