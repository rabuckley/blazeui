namespace BlazeUI.Sonner;

/// <summary>
/// Options for a promise-based toast that transitions through loading → success/error states.
/// </summary>
public sealed class PromiseToastOptions<T>
{
    /// <summary>
    /// Message shown while the promise is pending.
    /// </summary>
    public required string Loading { get; init; }

    /// <summary>
    /// Produces the success message from the resolved value.
    /// </summary>
    public required Func<T, string> Success { get; init; }

    /// <summary>
    /// Produces the error message from the caught exception.
    /// </summary>
    public required Func<Exception, string> Error { get; init; }

    /// <summary>
    /// Optional description shown below the title.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Optional callback invoked after the promise settles (success or error).
    /// </summary>
    public Action? Finally { get; init; }

    /// <summary>
    /// Duration in milliseconds for the success/error toast. Defaults to the Toaster's duration.
    /// </summary>
    public int? Duration { get; init; }
}
