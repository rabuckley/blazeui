using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.NumberField;

internal sealed class NumberFieldContext
{
    public double? Value { get; set; }
    public double? Min { get; set; }
    public double? Max { get; set; }
    public double Step { get; set; } = 1;

    /// <summary>Small step for Alt+Arrow key. Defaults to 0.1.</summary>
    public double SmallStep { get; set; } = 0.1;

    public double LargeStep { get; set; } = 10;
    public bool Disabled { get; set; }
    public bool ReadOnly { get; set; }
    public bool Required { get; set; }
    public bool IsScrubbing { get; set; }
    public string InputId { get; set; } = "";

    public Func<double?, Task> SetValue { get; set; } = _ => Task.CompletedTask;
    public Func<int, Task> StepValue { get; set; } = _ => Task.CompletedTask;

    /// <summary>Called by ScrubArea when a scrub gesture begins.</summary>
    public Action SetScrubbing { get; set; } = () => { };

    /// <summary>Called by ScrubArea when a scrub gesture ends.</summary>
    public Action ClearScrubbing { get; set; } = () => { };

    /// <summary>Stable key for JS dispose() to clean up only this root's registrations.</summary>
    public string InstanceKey { get; set; } = "";

    public IJSObjectReference? JsModule { get; set; }
    public DotNetObjectReference<NumberFieldRoot>? DotNetRef { get; set; }
}
