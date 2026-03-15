namespace BlazeUI.Headless.Overlay.Dialog;

/// <summary>
/// The result of a dialog interaction.
/// </summary>
public sealed class DialogResult
{
    private DialogResult(bool canceled, object? data)
    {
        Canceled = canceled;
        Data = data;
    }

    /// <summary>
    /// Whether the dialog was dismissed without an explicit result.
    /// </summary>
    public bool Canceled { get; }

    /// <summary>
    /// Optional data returned by the dialog.
    /// </summary>
    public object? Data { get; }

    public static DialogResult Ok(object? data = null) => new(false, data);
    public static DialogResult Cancel() => new(true, null);
}
