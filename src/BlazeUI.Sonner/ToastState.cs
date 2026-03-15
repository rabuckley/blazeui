using Microsoft.AspNetCore.Components;

namespace BlazeUI.Sonner;

/// <summary>
/// Mutable state for a single toast instance, tracked by the service and consumed by components.
/// </summary>
public sealed class ToastState
{
    public required string Id { get; init; }
    public required string Message { get; set; }
    public string? Description { get; set; }
    public ToastType Type { get; set; }
    public int Duration { get; set; }
    public bool Dismissible { get; set; } = true;
    public RenderFragment? Icon { get; set; }
    public RenderFragment? CustomContent { get; set; }
    public ToastAction? Action { get; set; }
    public ToastAction? Cancel { get; set; }
    public Action? OnDismiss { get; set; }
    public Action? OnAutoClose { get; set; }
    public string? Class { get; set; }
    public string? Style { get; set; }
    public Position? Position { get; set; }
    public bool Important { get; set; }
    public bool? RichColors { get; set; }

    /// <summary>
    /// Set by the service when dismiss is requested, triggers exit animation in the component.
    /// </summary>
    public bool MarkedForDeletion { get; set; }

    /// <summary>
    /// Measured height of the rendered toast element, reported back from JS.
    /// Used by the Toaster to compute stacking offsets.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Timestamp when the toast was created.
    /// </summary>
    public long CreatedAt { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    /// <summary>
    /// True when this toast was created by a promise call and is in the loading state.
    /// </summary>
    public bool IsPromise { get; set; }
}
