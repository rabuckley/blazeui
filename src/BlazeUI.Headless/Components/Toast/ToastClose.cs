using BlazeUI.Headless.Core;
using BlazeUI.Headless.Overlay.Toast;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Toast;

/// <summary>
/// Button that dismisses the enclosing toast when clicked. Renders a <c>&lt;button&gt;</c> element.
/// </summary>
public class ToastClose : BlazeElement<ToastCloseState>
{
    [Inject] private IToastService ToastServicePublic { get; set; } = default!;

    [CascadingParameter] internal ToastContext? Context { get; set; }

    protected override string DefaultTag => "button";

    protected override ToastCloseState GetCurrentState() => new(Context?.ToastType);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        if (Context?.ToastType is not null)
            yield return new("data-type", Context.ToastType);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        if (Context is not null)
        {
            yield return new("onclick",
                EventCallback.Factory.Create<MouseEventArgs>(this,
                    () => ToastServicePublic.Dismiss(Context.ToastId)));
        }
    }
}

/// <summary>State exposed to <see cref="ToastClose"/>'s class/style builders.</summary>
public readonly record struct ToastCloseState(string? Type);
