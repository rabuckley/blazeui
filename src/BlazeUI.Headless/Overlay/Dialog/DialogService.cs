using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Overlay.Dialog;

/// <summary>
/// Scoped dialog service implementation. A <see cref="BlazeDialogProvider"/> must be
/// rendered in the component tree for dialogs to appear.
/// </summary>
internal sealed class DialogService : IDialogService, IDisposable
{
    private bool _providerRegistered;
    private readonly List<DialogReference> _activeDialogs = [];

    /// <summary>
    /// Raised when a new dialog should be shown.
    /// </summary>
    internal event Action<DialogReference>? OnShow;

    /// <summary>
    /// Raised when a dialog has been dismissed.
    /// </summary>
    internal event Action<DialogReference>? OnClose;

    /// <summary>
    /// Registers a dialog provider. Throws if a provider is already registered
    /// to prevent duplicate rendering.
    /// </summary>
    internal void RegisterProvider()
    {
        if (_providerRegistered)
        {
            throw new InvalidOperationException(
                "A BlazeDialogProvider is already registered. Only one provider should exist in the component tree.");
        }

        _providerRegistered = true;
    }

    /// <summary>
    /// Unregisters the dialog provider, typically on dispose.
    /// </summary>
    internal void UnregisterProvider()
    {
        _providerRegistered = false;
    }

    public Task<DialogResult> ShowAsync<TComponent>(DialogParameters? parameters = null)
        where TComponent : ComponentBase
    {
        var reference = new DialogReference(
            Guid.NewGuid().ToString("N"),
            typeof(TComponent),
            parameters ?? new DialogParameters());

        _activeDialogs.Add(reference);
        OnShow?.Invoke(reference);

        return reference.Result;
    }

    /// <summary>
    /// Called by the provider when a dialog is dismissed.
    /// </summary>
    internal void Close(DialogReference reference, DialogResult result)
    {
        reference.Dismiss(result);
        _activeDialogs.Remove(reference);
        OnClose?.Invoke(reference);
    }

    public void Dispose()
    {
        // Cancel all pending dialogs to prevent memory leaks.
        foreach (var dialog in _activeDialogs.ToList())
        {
            dialog.Cancel();
        }

        _activeDialogs.Clear();
    }
}
