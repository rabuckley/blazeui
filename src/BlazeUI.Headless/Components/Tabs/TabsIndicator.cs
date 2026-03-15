using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Tabs;

/// <summary>
/// A visual indicator that slides to highlight the active tab. Renders a <c>&lt;span&gt;</c>
/// element with <c>role="presentation"</c>. The indicator's position and size are driven
/// by CSS custom properties set on the tab list by JS (<c>--active-tab-left</c>, etc.).
/// </summary>
public class TabsIndicator : BlazeElement<TabsIndicatorState>
{
    [CascadingParameter] internal TabsContext Context { get; set; } = default!;

    protected override string DefaultTag => "span";

    protected override TabsIndicatorState GetCurrentState() => new(Context.Orientation);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        var orientationValue = Context.Orientation is Orientation.Horizontal ? "horizontal" : "vertical";
        yield return new("data-orientation", orientationValue);
        yield return new("data-activation-direction", Context.ActivationDirection);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        // Presentation role removes the element from the accessibility tree — the
        // indicator is purely decorative.
        yield return new("role", "presentation");
    }
}

public readonly record struct TabsIndicatorState(Orientation Orientation);
