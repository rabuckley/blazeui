using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Toast;

/// <summary>
/// Positions a toast relative to an anchor element. Renders a <c>&lt;div&gt;</c> with
/// <c>role="presentation"</c> and positioning data attributes.
/// </summary>
/// <remarks>
/// Full floating-ui anchor positioning (as in Base UI's <c>ToastPositioner</c>) requires
/// JS interop for layout measurement. The current implementation renders the positioner
/// element with correct data attributes but does not apply CSS transform positioning.
/// TODO: integrate with the popup JS module to support anchored toast positioning.
/// </remarks>
public class ToastPositioner : BlazeElement<ToastPositionerState>
{
    /// <summary>Which side of the anchor element to place the toast on.</summary>
    [Parameter] public Side Side { get; set; } = Side.Top;

    /// <summary>Alignment of the toast along the anchor's cross-axis.</summary>
    [Parameter] public Align Align { get; set; } = Align.Center;

    protected override string DefaultTag => "div";

    protected override ToastPositionerState GetCurrentState()
    {
        // Derive the placement string and extract data-side / data-align from it.
        var placement = PlacementHelper.ToPlacement(Side, Align);
        return new(PlacementHelper.ToDataSide(placement), PlacementHelper.ToDataAlign(placement));
    }

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        var state = GetCurrentState();
        yield return new("data-side", state.Side);
        yield return new("data-align", state.Align);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        // role="presentation" signals that this is layout scaffolding, not a landmark.
        yield return new("role", "presentation");
    }
}

/// <summary>State exposed to <see cref="ToastPositioner"/>'s class/style builders.</summary>
public readonly record struct ToastPositionerState(string Side, string Align);
