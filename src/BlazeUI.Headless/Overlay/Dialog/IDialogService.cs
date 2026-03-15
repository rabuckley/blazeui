using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Overlay.Dialog;

/// <summary>
/// Service for programmatically showing dialogs.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows a dialog of type <typeparamref name="TComponent"/> and returns the result
    /// when the dialog is dismissed.
    /// </summary>
    Task<DialogResult> ShowAsync<TComponent>(DialogParameters? parameters = null)
        where TComponent : ComponentBase;
}
