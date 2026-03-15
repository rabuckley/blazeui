using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Toast;

/// <summary>
/// Renders the toast description text. Registers its ID with the parent
/// <see cref="ToastRoot"/> so the root can emit <c>aria-describedby</c>.
/// Renders a <c>&lt;p&gt;</c> element.
/// </summary>
public class ToastDescription : BlazeElement<ToastDescriptionState>
{
    [CascadingParameter] internal ToastContext? Context { get; set; }

    protected override string DefaultTag => "p";

    protected override string ElementId
    {
        get
        {
            var id = ResolvedId;
            // Register our ID with the root context once — the root re-renders after
            // receiving the ID and emits aria-describedby.
            if (Context is not null && Context.DescriptionId != id)
            {
                Context.DescriptionId = id;
                Context.NotifyChanged();
            }

            return id;
        }
    }

    protected override ToastDescriptionState GetCurrentState() => new(Context?.ToastType);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        if (Context?.ToastType is not null)
            yield return new("data-type", Context.ToastType);
    }
}

/// <summary>State exposed to <see cref="ToastDescription"/>'s class/style builders.</summary>
public readonly record struct ToastDescriptionState(string? Type);
