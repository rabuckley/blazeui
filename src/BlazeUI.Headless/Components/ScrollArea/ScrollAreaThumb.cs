using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.ScrollArea;

/// <summary>
/// The draggable part of the scrollbar that indicates the current scroll position.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
/// <remarks>
/// The thumb's size is set via the CSS custom properties
/// <c>--scroll-area-thumb-height</c> (vertical) and <c>--scroll-area-thumb-width</c>
/// (horizontal), which are updated in real-time by the JS module as the user scrolls
/// or the viewport/content is resized. The JS module also handles pointer-based
/// dragging and applies a <c>transform: translate3d</c> to position the thumb along
/// the track.
/// </remarks>
public class ScrollAreaThumb : BlazeElement<ScrollAreaThumbState>
{
    [CascadingParameter] internal ScrollAreaContext Context { get; set; } = default!;
    [CascadingParameter] internal ScrollbarOrientationContext? ScrollbarInfo { get; set; }

    protected override string DefaultTag => "div";
    protected override ScrollAreaThumbState GetCurrentState()
        => new(ScrollbarInfo?.Orientation ?? Orientation.Vertical);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        var orientation = ScrollbarInfo?.Orientation ?? Orientation.Vertical;
        yield return new("data-orientation", orientation is Orientation.Horizontal ? "horizontal" : "vertical");
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        // Consume the orientation-specific CSS variable written by the JS module onto the
        // parent scrollbar element. Using var() rather than a fixed px value means the
        // thumb resizes automatically as JS updates the property — no re-render needed.
        var orientation = ScrollbarInfo?.Orientation ?? Orientation.Vertical;
        var sizeStyle = orientation is Orientation.Vertical
            ? "height: var(--scroll-area-thumb-height);"
            : "width: var(--scroll-area-thumb-width);";
        yield return new("style", sizeStyle);
    }
}

/// <summary>State for <see cref="ScrollAreaThumb"/>.</summary>
public readonly record struct ScrollAreaThumbState(Orientation Orientation);
