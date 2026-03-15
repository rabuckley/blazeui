using Microsoft.AspNetCore.Components;

namespace BlazeUI.Sonner;

/// <summary>
/// Per-toast configuration options.
/// </summary>
public sealed class ToastOptions
{
    /// <summary>
    /// Replacement message (title) text. Used by <see cref="ISonnerService.Update"/> to change
    /// the primary text of an existing toast.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Secondary text displayed below the title.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Auto-dismiss duration in milliseconds. Defaults to the Toaster's duration (4000ms).
    /// Set to <see cref="int.MaxValue"/> to disable auto-dismiss.
    /// </summary>
    public int? Duration { get; init; }

    /// <summary>
    /// Overrides the toast type set by the convenience method.
    /// </summary>
    public ToastType? Type { get; init; }

    /// <summary>
    /// Whether the toast can be dismissed by the user. Defaults to true.
    /// </summary>
    public bool? Dismissible { get; init; }

    /// <summary>
    /// Custom icon rendered in the icon slot.
    /// </summary>
    public RenderFragment? Icon { get; init; }

    /// <summary>
    /// Primary action button (e.g. "Undo").
    /// </summary>
    public ToastAction? Action { get; init; }

    /// <summary>
    /// Cancel button (e.g. "Dismiss").
    /// </summary>
    public ToastAction? Cancel { get; init; }

    /// <summary>
    /// Callback invoked when the toast is dismissed by the user (close button, swipe).
    /// </summary>
    public Action? OnDismiss { get; init; }

    /// <summary>
    /// Callback invoked when the toast is auto-closed after its duration expires.
    /// </summary>
    public Action? OnAutoClose { get; init; }

    /// <summary>
    /// Additional CSS class applied to the toast element.
    /// </summary>
    public string? Class { get; init; }

    /// <summary>
    /// Inline styles applied to the toast element.
    /// </summary>
    public string? Style { get; init; }

    /// <summary>
    /// Per-toast position override. Null uses the Toaster's default position.
    /// </summary>
    public Position? Position { get; init; }

    /// <summary>
    /// When true, this toast renders above other toasts regardless of creation order.
    /// </summary>
    public bool? Important { get; init; }

    /// <summary>
    /// Use rich (saturated) colors for typed toasts.
    /// </summary>
    public bool? RichColors { get; init; }
}
