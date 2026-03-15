using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Tabs;

/// <summary>
/// Container for <see cref="TabsTab"/> elements. Renders a <c>&lt;div&gt;</c> element
/// with <c>role="tablist"</c> and manages roving tabindex keyboard navigation.
/// </summary>
public class TabsList : BlazeElement<TabsListState>
{
    [CascadingParameter] internal TabsContext Context { get; set; } = default!;

    /// <summary>
    /// Whether to automatically activate a tab when it receives keyboard focus via arrow keys.
    /// When <c>false</c> (the default), the user must press Enter or Space to activate.
    /// </summary>
    [Parameter] public bool ActivateOnFocus { get; set; }

    /// <summary>
    /// Whether keyboard focus wraps from the last tab back to the first (and vice versa).
    /// Defaults to <c>true</c>.
    /// </summary>
    [Parameter] public bool LoopFocus { get; set; } = true;

    protected override string DefaultTag => "div";
    protected override TabsListState GetCurrentState() => new(Context.Orientation);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        var orientationValue = Context.Orientation is Orientation.Horizontal ? "horizontal" : "vertical";
        yield return new("data-orientation", orientationValue);
        yield return new("data-activation-direction", Context.ActivationDirection);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        // The id is used by JS to scope keyboard navigation and indicator measurement.
        yield return new("id", Context.TabListId);
        yield return new("role", "tablist");

        // Base UI only emits aria-orientation for vertical; horizontal is the implicit default.
        if (Context.Orientation is Orientation.Vertical)
            yield return new("aria-orientation", "vertical");
    }
}

public readonly record struct TabsListState(Orientation Orientation);
