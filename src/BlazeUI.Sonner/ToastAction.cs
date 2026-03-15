namespace BlazeUI.Sonner;

/// <summary>
/// An action or cancel button displayed on a toast.
/// </summary>
public sealed class ToastAction
{
    /// <summary>
    /// The button label text.
    /// </summary>
    public required string Label { get; init; }

    /// <summary>
    /// Callback invoked when the button is clicked.
    /// </summary>
    public required Action OnClick { get; init; }
}
