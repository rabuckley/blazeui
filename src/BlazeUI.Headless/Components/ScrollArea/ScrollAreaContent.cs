using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.ScrollArea;

/// <summary>
/// A container for the content of the scroll area.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
/// <remarks>
/// <c>min-width: fit-content</c> prevents the content from shrinking below its
/// intrinsic size when the horizontal scrollbar is present, matching Base UI's
/// behaviour. A <c>role="presentation"</c> is added because this element has no
/// semantic meaning beyond layout.
/// </remarks>
public class ScrollAreaContent : BlazeElement<ScrollAreaContentState>
{
    protected override string DefaultTag => "div";
    protected override ScrollAreaContentState GetCurrentState() => default;
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes() => [];

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("role", "presentation");
        yield return new("style", "min-width: fit-content;");
    }
}

/// <summary>State for <see cref="ScrollAreaContent"/>.</summary>
public readonly record struct ScrollAreaContentState;
