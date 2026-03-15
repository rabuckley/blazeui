using Microsoft.JSInterop;

namespace BlazeUI.Bridge;

/// <summary>
/// Scoped queue that collects <see cref="BrowserMutation"/> instances during a render cycle
/// and executes them in batch during <c>OnAfterRenderAsync</c>. Keyed by
/// <see cref="BrowserMutation.ElementId"/> with last-write-wins semantics so that
/// rapid open/close toggles collapse to the final intended state.
/// </summary>
public sealed class BrowserMutationQueue
{
    private readonly Dictionary<string, BrowserMutation> _pending = new();

    /// <summary>
    /// Enqueues a mutation, replacing any existing mutation for the same element.
    /// </summary>
    public void Enqueue(BrowserMutation mutation)
    {
        _pending[mutation.ElementId] = mutation;
    }

    /// <summary>
    /// Snapshots and clears the queue, then executes each mutation.
    /// <see cref="JSDisconnectedException"/> and <see cref="OperationCanceledException"/>
    /// are swallowed (the Blazor circuit is dead). All other exceptions propagate.
    /// </summary>
    /// <remarks>
    /// Multiple calls per render cycle are expected and idempotent. The snapshot-and-clear
    /// design means the first call processes all pending mutations and subsequent calls
    /// hit the empty-queue guard immediately.
    /// </remarks>
    public async Task FlushAsync()
    {
        if (_pending.Count is 0)
            return;

        // Snapshot + clear so mutations enqueued during execution go to the next cycle.
        var batch = _pending.Values.ToArray();
        _pending.Clear();

        foreach (var mutation in batch)
        {
            try
            {
                await mutation.ExecuteAsync();
            }
            catch (JSDisconnectedException) { }
            catch (OperationCanceledException) { }
        }
    }
}
