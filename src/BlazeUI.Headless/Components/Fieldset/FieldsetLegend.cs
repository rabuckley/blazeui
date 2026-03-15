using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Fieldset;

/// <summary>
/// Renders the accessible label for a fieldset. Despite the name, Base UI renders this as
/// a <c>&lt;div&gt;</c> (not a native <c>&lt;legend&gt;</c>) and wires it to the root via
/// <c>aria-labelledby</c>. The component registers its resolved ID with the parent
/// <see cref="FieldsetRoot"/> so the root can set <c>aria-labelledby</c> automatically.
/// </summary>
public class FieldsetLegend : BlazeElement<FieldsetLegendState>, IDisposable
{
    [CascadingParameter]
    internal FieldsetContext? Context { get; set; }

    // Base UI renders FieldsetLegend as a <div>, not a native <legend>.
    protected override string DefaultTag => "div";

    protected override FieldsetLegendState GetCurrentState() =>
        new(Context?.Disabled ?? false);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-disabled", (Context?.Disabled ?? false) ? "" : null);
    }

    protected override void OnParametersSet()
    {
        // Register this legend's ID with the root so it can set aria-labelledby.
        Context?.SetLegendId(ElementId);
    }

    public void Dispose()
    {
        // Unregister on unmount so the root removes aria-labelledby when the legend is gone.
        Context?.SetLegendId(null);
    }
}

public readonly record struct FieldsetLegendState(bool Disabled);
