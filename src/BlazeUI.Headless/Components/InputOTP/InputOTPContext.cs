namespace BlazeUI.Headless.Components.InputOTP;

/// <summary>
/// Cascaded context that provides slot state to <see cref="InputOTPSlot"/> children.
/// </summary>
public sealed class InputOTPContext
{
    public InputOTPSlotState[] Slots { get; set; } = [];
    public bool IsFocused { get; set; }

    /// <summary>
    /// The container element ID. Slots use this to focus the hidden input on click.
    /// </summary>
    public string ContainerId { get; set; } = "";

    /// <summary>
    /// Delegate that child components call to focus the hidden input.
    /// </summary>
    public Action FocusInput { get; set; } = () => { };
}
