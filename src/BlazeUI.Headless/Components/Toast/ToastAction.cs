using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Toast;

/// <summary>
/// An action button rendered inside a toast. Renders a <c>&lt;button&gt;</c> element.
/// </summary>
public class ToastAction : BlazeElement<ToastActionState>
{
    [CascadingParameter] internal ToastContext? Context { get; set; }

    protected override string DefaultTag => "button";

    protected override ToastActionState GetCurrentState() => new(Context?.ToastType);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        if (Context?.ToastType is not null)
            yield return new("data-type", Context.ToastType);
    }
}

/// <summary>State exposed to <see cref="ToastAction"/>'s class/style builders.</summary>
public readonly record struct ToastActionState(string? Type);
