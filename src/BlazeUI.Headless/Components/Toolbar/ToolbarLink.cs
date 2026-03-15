using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Toolbar;

/// <summary>
/// An anchor link that participates in toolbar keyboard navigation.
/// Links cannot be disabled — they remain focusable regardless of the toolbar or group disabled state.
/// </summary>
public class ToolbarLink : BlazeElement<ToolbarLinkState>
{
    [CascadingParameter] internal ToolbarContext Context { get; set; } = default!;

    protected override string DefaultTag => "a";
    protected override ToolbarLinkState GetCurrentState() => new(Context.Orientation);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-orientation", Context.Orientation is Orientation.Horizontal ? "horizontal" : "vertical");
    }
}

public readonly record struct ToolbarLinkState(Orientation Orientation);
