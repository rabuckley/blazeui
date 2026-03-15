namespace BlazeUI.Bridge;

/// <summary>
/// A deferred DOM mutation to be executed after Blazor's render cycle completes.
/// Subclasses capture the JS module and arguments at enqueue time; <see cref="ExecuteAsync"/>
/// runs during <c>OnAfterRenderAsync</c> when the DOM is stable.
/// </summary>
public abstract class BrowserMutation
{
    public required string ElementId { get; init; }

    public abstract Task ExecuteAsync();
}
