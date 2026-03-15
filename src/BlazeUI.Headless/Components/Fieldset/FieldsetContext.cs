namespace BlazeUI.Headless.Components.Fieldset;

/// <summary>
/// Cascaded context from <see cref="FieldsetRoot"/> to child components.
/// Carries the disabled state and the mechanism for <see cref="FieldsetLegend"/> to
/// register its ID so the root can set <c>aria-labelledby</c>.
/// </summary>
internal sealed class FieldsetContext
{
    private readonly Action _onChanged;

    public FieldsetContext(Action onChanged)
    {
        _onChanged = onChanged;
    }

    public bool Disabled { get; set; }

    /// <summary>
    /// The ID of the rendered legend element, set by <see cref="FieldsetLegend"/> on mount.
    /// When non-null, <see cref="FieldsetRoot"/> emits <c>aria-labelledby</c> with this value.
    /// </summary>
    public string? LegendId { get; private set; }

    /// <summary>
    /// Called by <see cref="FieldsetLegend"/> during <c>OnParametersSet</c> to register its ID.
    /// Triggers a root re-render so <c>aria-labelledby</c> reflects the legend's ID.
    /// </summary>
    public void SetLegendId(string? id)
    {
        if (LegendId == id) return;
        LegendId = id;
        _onChanged();
    }
}
