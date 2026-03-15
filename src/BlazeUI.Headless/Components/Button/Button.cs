using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Button;

/// <summary>
/// A button component that can be used to trigger actions.
/// Renders a <c>&lt;button&gt;</c> element by default.
/// </summary>
public class Button : BlazeElement<ButtonState>
{
    /// <summary>
    /// Whether the button should ignore user interaction.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Whether the button should remain keyboard-focusable even when disabled.
    /// Useful for showing tooltips on disabled buttons.
    /// </summary>
    [Parameter]
    public bool FocusableWhenDisabled { get; set; }

    protected override string DefaultTag => "button";

    protected override ButtonState GetCurrentState() => new(Disabled);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-disabled", Disabled ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        var isNativeButton = (As ?? DefaultTag) is "button";

        // type="button" is handled by BlazeElement.BuildAttributes for all button tags.
        // Non-native elements need role="button" for screen reader semantics, along with
        // explicit tabindex management since they have no native focusability.
        if (!isNativeButton)
        {
            yield return new("role", "button");

            // Disabled non-native: remove from tab order unless focusable-when-disabled.
            // Enabled non-native: always in tab order at position 0.
            if (!Disabled || FocusableWhenDisabled)
                yield return new("tabindex", "0");
            else
                yield return new("tabindex", "-1");
        }

        // Disabled state:
        // - native + focusableWhenDisabled → aria-disabled keeps it in the tab order
        // - native + not focusableWhenDisabled → native disabled attribute removes it
        // - non-native → always use aria-disabled (no native disabled semantics)
        if (Disabled)
        {
            if (FocusableWhenDisabled || !isNativeButton)
                yield return new("aria-disabled", "true");
            else
                yield return new("disabled", "");
        }
    }
}

/// <summary>Represents the current state of a <see cref="Button"/>.</summary>
public readonly record struct ButtonState(bool Disabled);
