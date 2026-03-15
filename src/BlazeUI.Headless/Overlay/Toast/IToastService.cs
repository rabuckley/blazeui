using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Overlay.Toast;

/// <summary>
/// Service for programmatically showing toast notifications.
/// </summary>
public interface IToastService
{
    /// <summary>
    /// Shows a toast notification using the specified component type.
    /// </summary>
    void Show<TComponent>(ToastParameters? parameters = null)
        where TComponent : ComponentBase;

    /// <summary>
    /// Dismisses the toast with the specified ID. No-op if the toast is not found.
    /// </summary>
    void Dismiss(string toastId);
}
