namespace BlazeUI.Headless.Overlay.Toast;

/// <summary>
/// Configuration for showing a toast notification.
/// </summary>
public sealed class ToastParameters
{
    /// <summary>
    /// Toast message content.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Auto-dismiss timeout. When <c>null</c>, the toast remains until manually dismissed.
    /// </summary>
    public TimeSpan? Timeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Arbitrary key-value data passed to the toast component.
    /// </summary>
    public Dictionary<string, object?> Data { get; set; } = [];
}
