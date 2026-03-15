namespace BlazeUI.Headless.Core;

/// <summary>
/// Event arguments for BlazeUI component events, supporting cancellation
/// and propagation control.
/// </summary>
public sealed class BlazeEventArgs<T>
{
    public BlazeEventArgs(T value, string? reason = null)
    {
        Value = value;
        Reason = reason;
    }

    /// <summary>
    /// The event value.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// An optional reason string describing why the event occurred (e.g. "escape-key", "click-away").
    /// </summary>
    public string? Reason { get; }

    /// <summary>
    /// When set to <c>true</c>, the component should not apply the state change.
    /// </summary>
    public bool IsCanceled { get; private set; }

    /// <summary>
    /// When set to <c>true</c>, the event should propagate to parent components.
    /// By default, events do not propagate.
    /// </summary>
    public bool ShouldPropagate { get; private set; }

    /// <summary>
    /// Cancels the event, preventing the state change from being applied.
    /// </summary>
    public void Cancel() => IsCanceled = true;

    /// <summary>
    /// Allows the event to propagate to parent components.
    /// </summary>
    public void AllowPropagation() => ShouldPropagate = true;
}
