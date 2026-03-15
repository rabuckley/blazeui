namespace BlazeUI.Headless.Overlay.Toast;

/// <summary>
/// Tracks a single active toast and its auto-dismiss cancellation.
/// </summary>
internal sealed class ToastReference
{
    public ToastReference(string id, Type componentType, ToastParameters parameters)
    {
        Id = id;
        ComponentType = componentType;
        Parameters = parameters;
    }

    public string Id { get; }
    public Type ComponentType { get; }
    public ToastParameters Parameters { get; }

    /// <summary>
    /// Cancellation source for the auto-dismiss timer.
    /// </summary>
    internal CancellationTokenSource? DismissCts { get; set; }
}
