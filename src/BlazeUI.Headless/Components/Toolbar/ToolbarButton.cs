using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Toolbar;

public class ToolbarButton : BlazeElement<ToolbarButtonState>
{
    [CascadingParameter] internal ToolbarContext Context { get; set; } = default!;

    // Optional — only present when this button is inside a ToolbarGroup.
    [CascadingParameter] internal ToolbarGroupContext? GroupContext { get; set; }

    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// When <c>true</c> (the default), the button remains reachable via keyboard navigation
    /// even when disabled. This mirrors Base UI's <c>focusableWhenDisabled</c> behavior,
    /// allowing screen-reader users to discover and read the button's label.
    /// </summary>
    [Parameter] public bool FocusableWhenDisabled { get; set; } = true;

    // Disabled state aggregates: root → group → own prop (last wins).
    private bool EffectiveDisabled => Context.Disabled || (GroupContext?.Disabled ?? false) || Disabled;

    protected override string DefaultTag => "button";
    protected override ToolbarButtonState GetCurrentState() => new(EffectiveDisabled, Context.Orientation, FocusableWhenDisabled);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-disabled", EffectiveDisabled ? "" : null);
        yield return new("data-orientation", Context.Orientation is Orientation.Horizontal ? "horizontal" : "vertical");
        yield return new("data-focusable", FocusableWhenDisabled ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        if (EffectiveDisabled)
        {
            // When focusable-when-disabled, use aria-disabled rather than the native disabled
            // attribute so keyboard navigation can still reach the button.
            if (FocusableWhenDisabled)
            {
                yield return new("aria-disabled", "true");
            }
            else
            {
                yield return new("disabled", "true");
            }
        }
    }
}

public readonly record struct ToolbarButtonState(bool Disabled, Orientation Orientation, bool FocusableWhenDisabled);
