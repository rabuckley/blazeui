namespace BlazeUI.Headless.Components.Toast;

/// <summary>
/// Cascaded from <see cref="ToastRoot"/> to its children, providing toast state
/// and element IDs needed for ARIA attribute wiring.
/// </summary>
internal sealed class ToastContext
{
    /// <summary>The toast ID, used by <see cref="ToastClose"/> to dismiss this toast.</summary>
    public string ToastId { get; set; } = "";

    /// <summary>
    /// Application-defined type string (e.g. "success", "error").
    /// Emitted as <c>data-type</c> on sub-parts.
    /// </summary>
    public string? ToastType { get; set; }

    /// <summary>
    /// ID of the <see cref="ToastTitle"/> element. Set by the title itself so that
    /// <see cref="ToastRoot"/> can emit <c>aria-labelledby</c> once the title renders.
    /// </summary>
    public string? TitleId { get; set; }

    /// <summary>
    /// ID of the <see cref="ToastDescription"/> element. Set by the description so that
    /// <see cref="ToastRoot"/> can emit <c>aria-describedby</c>.
    /// </summary>
    public string? DescriptionId { get; set; }

    /// <summary>Whether the toasts in the viewport are expanded (e.g. on hover/focus).</summary>
    public bool Expanded { get; set; }

    /// <summary>
    /// The zero-based visible position index of this toast in the stack.
    /// Zero is the frontmost toast; a value greater than zero means the toast is behind others.
    /// </summary>
    public int VisibleIndex { get; set; }

    /// <summary>Delegate back to the root to trigger a re-render after title/description IDs are set.</summary>
    public Action NotifyChanged { get; set; } = () => { };
}
