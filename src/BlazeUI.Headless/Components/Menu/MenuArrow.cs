using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Menu;

/// <summary>
/// Displays an element positioned against the menu anchor (the arrow/caret).
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
public class MenuArrow : BlazeElement<MenuArrowState>
{
    [CascadingParameter] internal MenuContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";
    protected override MenuArrowState GetCurrentState() => new(Context.Open, Context.PlacementSide, Context.PlacementAlign);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-open", Context.Open ? "" : null);
        yield return new("data-closed", !Context.Open ? "" : null);
        yield return new("data-side", Context.PlacementSide);
        yield return new("data-align", Context.PlacementAlign);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("aria-hidden", "true");
    }
}

public readonly record struct MenuArrowState(bool Open, string? Side, string? Align);
