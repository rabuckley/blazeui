using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Toast;

/// <summary>
/// Container that holds all toast notifications. Acts as an ARIA live region so screen
/// readers announce new toasts as they appear. Renders a <c>&lt;div&gt;</c> element with
/// <c>role="region"</c>, <c>aria-live="polite"</c>, and related accessibility attributes.
/// </summary>
public class ToastViewport : BlazeElement<ToastViewportState>
{
    /// <summary>
    /// Whether the toast stack is in its expanded state (e.g. while the user hovers or
    /// keyboard-focuses the viewport). Emitted as <c>data-expanded</c>.
    /// </summary>
    [Parameter]
    public bool Expanded { get; set; }

    protected override string DefaultTag => "div";

    protected override ToastViewportState GetCurrentState() => new(Expanded);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-expanded", Expanded ? (object?)"" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        // These attributes match Base UI's ToastViewport ARIA contract:
        //   role="region"          — landmark so assistive tech can navigate to it
        //   aria-live="polite"     — new toasts announced without interrupting the user
        //   aria-atomic="false"    — only changed content announced, not the full region
        //   aria-relevant=...      — notify on additions and text changes only
        //   aria-label             — accessible name for the landmark
        //   tabindex="-1"          — programmatically focusable but not in the tab order
        yield return new("role", "region");
        yield return new("aria-live", "polite");
        yield return new("aria-atomic", "false");
        yield return new("aria-relevant", "additions text");
        yield return new("aria-label", "Notifications");
        yield return new("tabindex", "-1");
    }
}

/// <summary>State exposed to <see cref="ToastViewport"/>'s class/style builders.</summary>
public readonly record struct ToastViewportState(bool Expanded);
