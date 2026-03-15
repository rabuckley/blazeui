using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Avatar;

/// <summary>
/// The avatar fallback element, displayed when the image has not yet loaded or
/// has failed to load. Renders as a <c>&lt;span&gt;</c> by default.
/// </summary>
public class AvatarFallback : BlazeElement<AvatarState>
{
    /// <summary>
    /// Delay in milliseconds before showing the fallback. Useful for preventing
    /// a flash of the fallback when the image loads quickly.
    /// </summary>
    [Parameter]
    public int? Delay { get; set; }

    [CascadingParameter]
    internal AvatarContext Context { get; set; } = default!;

    private bool _delayElapsed;

    protected override string DefaultTag => "span";

    protected override AvatarState GetCurrentState() => new(Context.Status);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-status", Context.Status.ToString().ToLowerInvariant());
    }

    protected override async Task OnInitializedAsync()
    {
        if (Delay.HasValue && Delay.Value > 0)
        {
            await Task.Delay(Delay.Value);
            _delayElapsed = true;
        }
        else
        {
            _delayElapsed = true;
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!_delayElapsed)
            return;

        // The fallback is visible whenever the image is not in a loaded state — this
        // covers idle (no image provided), loading (image in flight), and error states.
        if (Context.Status is AvatarLoadingStatus.Loaded)
            return;

        base.BuildRenderTree(builder);
    }
}
