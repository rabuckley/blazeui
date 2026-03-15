using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Toolbar;

/// <summary>
/// An input element that participates in toolbar keyboard navigation.
/// </summary>
public class ToolbarInput : BlazeElement<ToolbarInputState>
{
    [CascadingParameter] internal ToolbarContext Context { get; set; } = default!;

    // Optional — only present when this input is inside a ToolbarGroup.
    [CascadingParameter] internal ToolbarGroupContext? GroupContext { get; set; }

    [Parameter] public bool Disabled { get; set; }

    // Disabled state aggregates: root → group → own prop (last wins).
    private bool EffectiveDisabled => Context.Disabled || (GroupContext?.Disabled ?? false) || Disabled;

    protected override string DefaultTag => "input";
    protected override ToolbarInputState GetCurrentState() => new(EffectiveDisabled, Context.Orientation);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-disabled", EffectiveDisabled ? "" : null);
        yield return new("data-orientation", Context.Orientation is Orientation.Horizontal ? "horizontal" : "vertical");
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        if (EffectiveDisabled)
            yield return new("aria-disabled", "true");
    }
}

public readonly record struct ToolbarInputState(bool Disabled, Orientation Orientation);
