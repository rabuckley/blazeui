namespace BlazeUI.Headless.Core;

/// <summary>
/// Dual-mode state container: uncontrolled (internal) by default, controlled
/// when the consumer supplies a value. When controlled, internal mutations
/// via <see cref="SetInternal"/> are ignored.
/// </summary>
internal sealed class ComponentState<T>
{
    private T _internalValue;
    private T? _controlledValue;
    private bool _isControlled;

    public ComponentState(T defaultValue)
    {
        _internalValue = defaultValue;
    }

    /// <summary>
    /// The current effective value — controlled value when set, otherwise the internal value.
    /// </summary>
    public T Value => _isControlled ? _controlledValue! : _internalValue;

    /// <summary>
    /// Sets the externally-controlled value. Once called, internal mutations are ignored.
    /// </summary>
    public void SetControlled(T value)
    {
        _isControlled = true;
        _controlledValue = value;
    }

    /// <summary>
    /// Clears the controlled override, reverting to internal state management.
    /// </summary>
    public void ClearControlled()
    {
        _isControlled = false;
        _controlledValue = default;
    }

    /// <summary>
    /// Updates the internal value. Has no effect when the state is controlled.
    /// </summary>
    public void SetInternal(T value)
    {
        if (!_isControlled)
        {
            _internalValue = value;
        }
    }
}
