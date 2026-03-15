namespace BlazeUI.Headless.Components.InputOTP;

/// <summary>
/// Represents the visual state of a single OTP slot.
/// </summary>
public readonly record struct InputOTPSlotState(
    /// <summary>The character displayed in this slot, or <c>null</c> if empty.</summary>
    char? Char,
    /// <summary>Whether this slot shows the blinking caret.</summary>
    bool HasFakeCaret,
    /// <summary>Whether the selection/cursor includes this slot.</summary>
    bool IsActive
);
