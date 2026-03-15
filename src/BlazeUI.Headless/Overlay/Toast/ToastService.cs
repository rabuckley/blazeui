using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Overlay.Toast;

/// <summary>
/// Scoped toast service. A toast provider must be rendered for toasts to appear.
/// </summary>
internal sealed class ToastService : IToastService, IDisposable
{
    private bool _providerRegistered;
    private readonly List<ToastReference> _activeToasts = [];

    /// <summary>
    /// Raised when a new toast should be shown.
    /// </summary>
    internal event Action<ToastReference>? OnShow;

    /// <summary>
    /// Raised when a toast should be dismissed.
    /// </summary>
    internal event Action<ToastReference>? OnClose;

    /// <summary>
    /// Registers a toast provider. Throws if a provider is already registered
    /// to prevent duplicate rendering.
    /// </summary>
    internal void RegisterProvider()
    {
        if (_providerRegistered)
        {
            throw new InvalidOperationException(
                "A ToastProvider is already registered. Only one provider should exist in the component tree.");
        }

        _providerRegistered = true;
    }

    /// <summary>
    /// Unregisters the toast provider, typically on dispose.
    /// </summary>
    internal void UnregisterProvider()
    {
        _providerRegistered = false;
    }

    public void Show<TComponent>(ToastParameters? parameters = null)
        where TComponent : ComponentBase
    {
        var toast = new ToastReference(
            Guid.NewGuid().ToString("N"),
            typeof(TComponent),
            parameters ?? new ToastParameters());

        _activeToasts.Add(toast);
        OnShow?.Invoke(toast);

        // Auto-dismiss via Task.Delay if a timeout is configured.
        if (toast.Parameters.Timeout is { } timeout)
        {
            var cts = new CancellationTokenSource();
            toast.DismissCts = cts;
            _ = AutoDismissAsync(toast, timeout, cts.Token);
        }
    }

    private async Task AutoDismissAsync(ToastReference toast, TimeSpan timeout, CancellationToken ct)
    {
        try
        {
            await Task.Delay(timeout, ct);
            Close(toast);
        }
        catch (TaskCanceledException)
        {
            // Toast was manually dismissed before the timeout.
        }
    }

    public void Dismiss(string toastId)
    {
        var toast = _activeToasts.Find(t => t.Id == toastId);
        if (toast is not null)
        {
            Close(toast);
        }
    }

    /// <summary>
    /// Dismisses a toast.
    /// </summary>
    internal void Close(ToastReference toast)
    {
        toast.DismissCts?.Cancel();
        toast.DismissCts?.Dispose();
        _activeToasts.Remove(toast);
        OnClose?.Invoke(toast);
    }

    public void Dispose()
    {
        foreach (var toast in _activeToasts.ToList())
        {
            toast.DismissCts?.Cancel();
            toast.DismissCts?.Dispose();
        }

        _activeToasts.Clear();
    }
}
