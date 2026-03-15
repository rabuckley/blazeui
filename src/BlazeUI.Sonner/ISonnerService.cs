using Microsoft.AspNetCore.Components;

namespace BlazeUI.Sonner;

/// <summary>
/// Service for programmatically showing Sonner toast notifications.
/// Named <c>ISonnerService</c> (not <c>IToastService</c>) to avoid ambiguity
/// with <c>BlazeUI.Headless.Overlay.Toast.IToastService</c> when both libraries are referenced.
/// </summary>
public interface ISonnerService
{
    /// <summary>
    /// Shows a toast with the default (normal) style.
    /// </summary>
    string Show(string message, ToastOptions? options = null);

    /// <summary>
    /// Shows a success toast.
    /// </summary>
    string Success(string message, ToastOptions? options = null);

    /// <summary>
    /// Shows an error toast.
    /// </summary>
    string Error(string message, ToastOptions? options = null);

    /// <summary>
    /// Shows a warning toast.
    /// </summary>
    string Warning(string message, ToastOptions? options = null);

    /// <summary>
    /// Shows an info toast.
    /// </summary>
    string Info(string message, ToastOptions? options = null);

    /// <summary>
    /// Shows a loading toast (no auto-dismiss until updated or dismissed).
    /// </summary>
    string Loading(string message, ToastOptions? options = null);

    /// <summary>
    /// Shows a toast with fully custom render content.
    /// </summary>
    string Custom(RenderFragment content, ToastOptions? options = null);

    /// <summary>
    /// Shows a loading toast that transitions to success or error when the promise settles.
    /// </summary>
    string Promise<T>(Task<T> promise, PromiseToastOptions<T> options);

    /// <summary>
    /// Updates an existing toast's options (message, description, type, etc.).
    /// </summary>
    void Update(string toastId, ToastOptions options);

    /// <summary>
    /// Dismisses a specific toast, or all toasts if <paramref name="toastId"/> is null.
    /// </summary>
    void Dismiss(string? toastId = null);
}
