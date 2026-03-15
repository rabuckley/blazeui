namespace BlazeUI.Headless.Overlay.Dialog;

/// <summary>
/// Configuration for showing a dialog.
/// </summary>
public sealed class DialogParameters
{
    /// <summary>
    /// Dialog title, available for rendering by the dialog component.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Whether the dialog is modal (blocks interaction with content behind it).
    /// </summary>
    public bool Modal { get; set; } = true;

    /// <summary>
    /// Whether to trap focus within the dialog while it is open.
    /// </summary>
    public bool TrapFocus { get; set; } = true;

    /// <summary>
    /// Arbitrary key-value data passed to the dialog component.
    /// </summary>
    public Dictionary<string, object?> Data { get; set; } = [];
}
