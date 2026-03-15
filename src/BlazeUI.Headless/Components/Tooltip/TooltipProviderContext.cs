namespace BlazeUI.Headless.Components.Tooltip;

/// <summary>
/// Shared context cascaded by <see cref="TooltipProvider"/> to coordinate delay
/// behavior across adjacent tooltips. When one tooltip closes and another opens
/// within the <see cref="Timeout"/> window, the second opens instantly.
/// </summary>
internal sealed class TooltipProviderContext
{
    public int? Delay { get; set; }
    public int? CloseDelay { get; set; }

    /// <summary>
    /// Window in ms after a tooltip closes during which the next tooltip opens instantly.
    /// </summary>
    public int Timeout { get; set; } = 400;

    /// <summary>
    /// Timestamp (from <see cref="Environment.TickCount64"/>) of the most recent tooltip
    /// close across all children. Used by <see cref="TooltipRoot"/> to determine whether
    /// the open delay should be skipped.
    /// </summary>
    public long LastCloseTimestamp { get; set; }

    /// <summary>
    /// Records that a tooltip was just closed. Called by <see cref="TooltipRoot"/> during
    /// <see cref="TooltipRoot.SetOpenAsync"/> when transitioning to closed.
    /// </summary>
    public void RecordClose() => LastCloseTimestamp = Environment.TickCount64;

    /// <summary>
    /// Returns <c>true</c> if a tooltip was closed recently enough that the next tooltip
    /// should skip its open delay and appear instantly.
    /// </summary>
    public bool IsWithinGroupTimeout()
    {
        if (LastCloseTimestamp == 0) return false;
        var elapsed = Environment.TickCount64 - LastCloseTimestamp;
        return elapsed <= Timeout;
    }
}
