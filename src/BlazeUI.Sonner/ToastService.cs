using Microsoft.AspNetCore.Components;

namespace BlazeUI.Sonner;

/// <summary>
/// Scoped toast service managing all active Sonner toasts.
/// A <see cref="Toaster"/> component must be rendered for toasts to appear.
/// </summary>
internal sealed class ToastService : ISonnerService, IDisposable
{
    private const int DefaultDuration = 4000;
    private bool _providerRegistered;
    private readonly List<ToastState> _activeToasts = [];

    /// <summary>
    /// Raised on any mutation (add, update, dismiss, remove) so the Toaster can re-render.
    /// </summary>
    internal event Action? OnChange;

    /// <summary>
    /// Snapshot of active toasts for rendering.
    /// </summary>
    internal IReadOnlyList<ToastState> Toasts => _activeToasts;

    internal void RegisterProvider()
    {
        if (_providerRegistered)
        {
            throw new InvalidOperationException(
                "A Toaster is already registered. Only one Toaster should exist in the component tree.");
        }

        _providerRegistered = true;
    }

    internal void UnregisterProvider()
    {
        _providerRegistered = false;
    }

    public string Show(string message, ToastOptions? options = null)
        => CreateToast(message, ToastType.Normal, options);

    public string Success(string message, ToastOptions? options = null)
        => CreateToast(message, ToastType.Success, options);

    public string Error(string message, ToastOptions? options = null)
        => CreateToast(message, ToastType.Error, options);

    public string Warning(string message, ToastOptions? options = null)
        => CreateToast(message, ToastType.Warning, options);

    public string Info(string message, ToastOptions? options = null)
        => CreateToast(message, ToastType.Info, options);

    public string Loading(string message, ToastOptions? options = null)
        => CreateToast(message, ToastType.Loading, options);

    public string Custom(RenderFragment content, ToastOptions? options = null)
    {
        var toast = BuildToastState("", ToastType.Normal, options);
        toast.CustomContent = content;
        _activeToasts.Add(toast);
        OnChange?.Invoke();
        return toast.Id;
    }

    public string Promise<T>(Task<T> promise, PromiseToastOptions<T> options)
    {
        var toast = BuildToastState(options.Loading, ToastType.Loading, new ToastOptions
        {
            Description = options.Description,
        });
        toast.IsPromise = true;
        _activeToasts.Add(toast);
        OnChange?.Invoke();

        _ = RunPromiseAsync(promise, toast, options);
        return toast.Id;
    }

    public void Update(string toastId, ToastOptions options)
    {
        var toast = _activeToasts.Find(t => t.Id == toastId);
        if (toast is null) return;

        if (options.Message is not null) toast.Message = options.Message;
        if (options.Type is { } type) toast.Type = type;
        if (options.Description is not null) toast.Description = options.Description;
        if (options.Duration is { } duration) toast.Duration = duration;
        if (options.Icon is not null) toast.Icon = options.Icon;
        if (options.Action is not null) toast.Action = options.Action;
        if (options.Cancel is not null) toast.Cancel = options.Cancel;
        if (options.Class is not null) toast.Class = options.Class;
        if (options.Style is not null) toast.Style = options.Style;
        if (options.RichColors is not null) toast.RichColors = options.RichColors;
        if (options.Dismissible is { } dismissible) toast.Dismissible = dismissible;
        if (options.Important is { } important) toast.Important = important;

        OnChange?.Invoke();
    }

    public void Dismiss(string? toastId = null)
    {
        if (toastId is null)
        {
            foreach (var toast in _activeToasts)
            {
                toast.MarkedForDeletion = true;
            }
        }
        else
        {
            var toast = _activeToasts.Find(t => t.Id == toastId);
            if (toast is null) return;
            toast.MarkedForDeletion = true;
        }

        OnChange?.Invoke();
    }

    /// <summary>
    /// Called by the Toast component after the exit animation completes. Removes the toast from the list entirely.
    /// </summary>
    internal void Remove(string toastId)
    {
        var toast = _activeToasts.Find(t => t.Id == toastId);
        if (toast is null) return;

        _activeToasts.Remove(toast);
        OnChange?.Invoke();
    }

    private string CreateToast(string message, ToastType type, ToastOptions? options)
    {
        var toast = BuildToastState(message, type, options);
        _activeToasts.Add(toast);
        OnChange?.Invoke();
        return toast.Id;
    }

    private static ToastState BuildToastState(string message, ToastType type, ToastOptions? options)
    {
        return new ToastState
        {
            Id = Guid.NewGuid().ToString("N"),
            Message = message,
            Type = options?.Type ?? type,
            Description = options?.Description,
            Duration = options?.Duration ?? DefaultDuration,
            Dismissible = options?.Dismissible ?? true,
            Icon = options?.Icon,
            Action = options?.Action,
            Cancel = options?.Cancel,
            OnDismiss = options?.OnDismiss,
            OnAutoClose = options?.OnAutoClose,
            Class = options?.Class,
            Style = options?.Style,
            Position = options?.Position,
            Important = options?.Important ?? false,
            RichColors = options?.RichColors,
        };
    }

    private async Task RunPromiseAsync<T>(Task<T> promise, ToastState toast, PromiseToastOptions<T> options)
    {
        try
        {
            var result = await promise;
            toast.Message = options.Success(result);
            toast.Type = ToastType.Success;
            toast.IsPromise = false;
            if (options.Duration is { } duration) toast.Duration = duration;
        }
        catch (Exception ex)
        {
            toast.Message = options.Error(ex);
            toast.Type = ToastType.Error;
            toast.IsPromise = false;
            if (options.Duration is { } duration) toast.Duration = duration;
        }
        finally
        {
            options.Finally?.Invoke();
            OnChange?.Invoke();
        }
    }

    public void Dispose()
    {
        _activeToasts.Clear();
    }
}
