using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Toast;

/// <summary>
/// Container for toast content (title, description, and actions). Renders a <c>&lt;div&gt;</c> element.
/// </summary>
public class ToastContent : BlazeElement<ToastContentState>
{
    [CascadingParameter] internal ToastContext? Context { get; set; }

    protected override string DefaultTag => "div";

    protected override ToastContentState GetCurrentState()
    {
        var expanded = Context?.Expanded ?? false;
        // A toast is "behind" when it is not the frontmost item in the visual stack.
        // The visible index is zero for the front toast; anything above zero is behind.
        var behind = (Context?.VisibleIndex ?? 0) > 0;
        return new(expanded, behind);
    }

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        var expanded = Context?.Expanded ?? false;
        var behind = (Context?.VisibleIndex ?? 0) > 0;

        yield return new("data-expanded", expanded ? (object?)"" : null);
        yield return new("data-behind", behind ? (object?)"" : null);
    }
}

/// <summary>State exposed to <see cref="ToastContent"/>'s class/style builders.</summary>
public readonly record struct ToastContentState(bool Expanded, bool Behind);
