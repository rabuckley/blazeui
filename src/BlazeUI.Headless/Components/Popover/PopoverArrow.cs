using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Popover;

/// <summary>
/// Displays an element positioned against the popover anchor.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
public class PopoverArrow : BlazeElement<PopoverArrowState>
{
    [CascadingParameter]
    internal PopoverContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";

    protected override PopoverArrowState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        // Mirror Base UI's popupStateMapping for the arrow element.
        yield return new("data-open", Context.Open ? "" : null);
        yield return new("data-closed", !Context.Open ? "" : null);
        // data-side and data-align are set by JS positioning via startAutoUpdate.
        // data-uncentered is also set by JS when the arrow cannot be centered on the anchor.
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        // The arrow is purely decorative — hide it from the accessibility tree.
        yield return new("aria-hidden", "true");
    }
}

public readonly record struct PopoverArrowState(bool Open);
