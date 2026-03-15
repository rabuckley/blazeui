using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Combobox;

/// <summary>
/// Accessible label for the combobox. Registers its ID with the root context so the
/// input can reference it via <c>aria-labelledby</c>.
/// </summary>
public class ComboboxLabel : BlazeElement<ComboboxLabelState>
{
    [CascadingParameter] internal ComboboxContext Context { get; set; } = default!;

    private string _labelId = "";

    protected override string DefaultTag => "label";

    protected override void OnInitialized()
    {
        _labelId = IdGenerator.Next("combobox-label");
        Context.LabelId = _labelId;
    }

    protected override string ElementId => Id ?? _labelId;

    protected override void OnParametersSet()
    {
        // Propagate effective ID into context so ComboboxInput's
        // aria-labelledby points at the right element.
        Context.LabelId = Id ?? _labelId;
    }

    protected override ComboboxLabelState GetCurrentState() => new(Context.Disabled);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-disabled", Context.Disabled ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        // The label's `for` attribute points at the input so clicking the label focuses it.
        yield return new("for", Context.InputId);
    }
}

public readonly record struct ComboboxLabelState(bool Disabled);
