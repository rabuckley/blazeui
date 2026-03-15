using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Toolbar;

/// <summary>
/// A visual separator between toolbar items.
/// Its orientation is always perpendicular to the containing toolbar.
/// </summary>
public class ToolbarSeparator : BlazeElement<ToolbarSeparatorState>
{
    [CascadingParameter] internal ToolbarContext Context { get; set; } = default!;

    // Separator orientation is always perpendicular to the toolbar.
    private Orientation SeparatorOrientation =>
        Context.Orientation is Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal;

    protected override string DefaultTag => "div";
    protected override ToolbarSeparatorState GetCurrentState() => new(SeparatorOrientation);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-orientation", SeparatorOrientation is Orientation.Horizontal ? "horizontal" : "vertical");
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("role", "separator");
        yield return new("aria-orientation", SeparatorOrientation is Orientation.Horizontal ? "horizontal" : "vertical");
    }
}

public readonly record struct ToolbarSeparatorState(Orientation Orientation);
