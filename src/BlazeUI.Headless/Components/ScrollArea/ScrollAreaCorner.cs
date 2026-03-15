using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.ScrollArea;

/// <summary>
/// A small rectangular area that appears at the intersection of the horizontal and
/// vertical scrollbars.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
/// <remarks>
/// The JS module toggles this element's rendered state and updates its dimensions
/// (<c>width</c> and <c>height</c>) once both scrollbars have been measured. On
/// initial SSR the element always renders so the JS module can reference it by
/// <c>data-id</c>; the module hides it immediately if only one scrollbar is present.
/// </remarks>
public class ScrollAreaCorner : BlazeElement<ScrollAreaCornerState>
{
    [CascadingParameter] internal ScrollAreaContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";
    protected override ScrollAreaCornerState GetCurrentState() => new();
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes() => [];

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        // Positioned at the intersection of both scrollbar tracks.
        // Width and height are set by the JS module once it has measured the scrollbar dimensions.
        yield return new("style", "position: absolute; bottom: 0; inset-inline-end: 0;");
        yield return new("data-id", $"{Context.RootId}-corner");
    }
}

/// <summary>State for <see cref="ScrollAreaCorner"/>.</summary>
public readonly record struct ScrollAreaCornerState;
