using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Popover;

/// <summary>
/// A viewport for displaying content transitions when one popover is opened by multiple triggers
/// and switching between them is animated. Renders a <c>&lt;div&gt;</c> element.
/// </summary>
/// <remarks>
/// TODO: The full Base UI viewport behavior — detecting trigger-switch activation direction
/// and coordinating entering/exiting content fragments — requires JS measurement of
/// trigger positions that has not yet been implemented. This component renders the container
/// div but does not animate between payloads.
/// </remarks>
public class PopoverViewport : BlazeElement<PopoverViewportState>
{
    [CascadingParameter]
    internal PopoverContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";

    protected override PopoverViewportState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield break;
    }
}

public readonly record struct PopoverViewportState(bool Open);
