using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.PreviewCard;

/// <summary>
/// A viewport for displaying content transitions when one popup is shared across
/// multiple triggers. In Blazor, this is a passthrough container — the per-trigger
/// content animation managed by Base UI's <c>usePopupViewport</c> hook has no direct
/// equivalent in the Blazor component model.
/// </summary>
// TODO: Animated viewport transitions (data-current, data-previous, data-activation-direction)
// require server-side coordination across multiple trigger instances. This is deferred.
public class PreviewCardViewport : BlazeElement<PreviewCardViewportState>
{
    [CascadingParameter]
    internal PreviewCardContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";

    protected override PreviewCardViewportState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield break;
    }
}

public readonly record struct PreviewCardViewportState(bool Open);
