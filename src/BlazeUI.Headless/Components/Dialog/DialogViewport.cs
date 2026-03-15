using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Dialog;

/// <summary>
/// A scrollable positioning container for the dialog popup. Wrapping <see cref="DialogPopup"/>
/// in a viewport enables scroll-within-overlay layouts (e.g. a centered dialog inside a
/// full-screen scrollable layer). Carries <c>role="presentation"</c> so screen readers
/// treat it as structural scaffolding.
/// </summary>
/// <remarks>
/// BlazeUI does not implement nested-dialog tracking, so <c>data-nested</c> and
/// <c>data-nested-dialog-open</c> are not emitted. These are structural complexity
/// that adds little accessibility value and would require a parent-context nesting
/// counter analogous to Base UI's <c>nestedOpenDialogCount</c>.
/// </remarks>
public class DialogViewport : BlazeElement<DialogViewportState>
{
    [CascadingParameter]
    internal DialogContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";

    protected override DialogViewportState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-open", Context.Open ? "" : null);
        yield return new("data-closed", !Context.Open ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        // role="presentation" signals that this is layout scaffolding, not a landmark.
        yield return new("role", "presentation");
    }
}

public readonly record struct DialogViewportState(bool Open);
