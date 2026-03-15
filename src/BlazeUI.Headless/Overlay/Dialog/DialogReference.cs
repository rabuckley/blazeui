namespace BlazeUI.Headless.Overlay.Dialog;

/// <summary>
/// Tracks a single active dialog instance and its completion state.
/// </summary>
internal sealed class DialogReference
{
    private readonly TaskCompletionSource<DialogResult> _tcs = new();

    public DialogReference(string id, Type componentType, DialogParameters parameters)
    {
        Id = id;
        ComponentType = componentType;
        Parameters = parameters;
    }

    public string Id { get; }
    public Type ComponentType { get; }
    public DialogParameters Parameters { get; }

    /// <summary>
    /// A task that completes when the dialog is dismissed.
    /// </summary>
    public Task<DialogResult> Result => _tcs.Task;

    /// <summary>
    /// Dismisses the dialog with the given result.
    /// </summary>
    public void Dismiss(DialogResult result) => _tcs.TrySetResult(result);

    /// <summary>
    /// Cancels the dialog (e.g. when the provider is disposed).
    /// </summary>
    public void Cancel() => _tcs.TrySetResult(DialogResult.Cancel());
}
