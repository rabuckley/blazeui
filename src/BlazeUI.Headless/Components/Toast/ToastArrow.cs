using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Toast;

/// <summary>
/// Displays an arrow element positioned against the toast anchor.
/// Renders a <c>&lt;div&gt;</c> element with <c>aria-hidden="true"</c>.
/// </summary>
/// <remarks>
/// In Base UI, arrow positioning is driven by floating-ui layout values from the positioner.
/// BlazeUI exposes <see cref="Side"/> and <see cref="Align"/> as explicit parameters until
/// full floating-ui positioner integration is implemented.
/// </remarks>
public class ToastArrow : BlazeElement<ToastArrowState>
{
    /// <summary>Which side of the anchor the toast is placed on.</summary>
    [Parameter] public string Side { get; set; } = "top";

    /// <summary>How the toast is aligned relative to the specified side.</summary>
    [Parameter] public string Align { get; set; } = "center";

    /// <summary>Whether the arrow cannot be centered on the anchor.</summary>
    [Parameter] public bool Uncentered { get; set; }

    protected override string DefaultTag => "div";

    protected override ToastArrowState GetCurrentState() => new(Side, Align, Uncentered);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-side", Side);
        yield return new("data-align", Align);
        if (Uncentered)
            yield return new("data-uncentered", "");
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("aria-hidden", "true");
    }
}

/// <summary>State exposed to <see cref="ToastArrow"/>'s class/style builders.</summary>
public readonly record struct ToastArrowState(string Side, string Align, bool Uncentered);
