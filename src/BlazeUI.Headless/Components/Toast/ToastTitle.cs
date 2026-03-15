using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Toast;

/// <summary>
/// Renders the toast title. Registers its ID with the parent <see cref="ToastRoot"/>
/// so the root can emit <c>aria-labelledby</c>. Renders an <c>&lt;h2&gt;</c> element.
/// </summary>
public class ToastTitle : BlazeElement<ToastTitleState>
{
    [CascadingParameter] internal ToastContext? Context { get; set; }

    protected override string DefaultTag => "h2";

    protected override string ElementId
    {
        get
        {
            var id = ResolvedId;
            // Register our ID with the root context once — the root re-renders after
            // receiving the ID and emits aria-labelledby.
            if (Context is not null && Context.TitleId != id)
            {
                Context.TitleId = id;
                Context.NotifyChanged();
            }

            return id;
        }
    }

    protected override ToastTitleState GetCurrentState() => new(Context?.ToastType);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        if (Context?.ToastType is not null)
            yield return new("data-type", Context.ToastType);
    }
}

/// <summary>State exposed to <see cref="ToastTitle"/>'s class/style builders.</summary>
public readonly record struct ToastTitleState(string? Type);
