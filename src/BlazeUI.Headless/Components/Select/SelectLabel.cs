using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Select;

/// <summary>
/// An accessible label that is automatically associated with the select trigger.
/// Renders a <c>&lt;div&gt;</c> element. The trigger's <c>aria-labelledby</c>
/// attribute is wired to this label's id automatically.
/// </summary>
public class SelectLabel : BlazeElement<SelectLabelState>
{
    [CascadingParameter] internal SelectContext Context { get; set; } = default!;

    private string _labelId = "";

    protected override string DefaultTag => "div";

    protected override void OnInitialized()
    {
        _labelId = IdGenerator.Next("select-label");
        Context.LabelId = _labelId;
    }

    protected override string ElementId => Id ?? _labelId;

    protected override void OnParametersSet()
    {
        // Propagate effective ID into context so SelectTrigger's
        // aria-labelledby points at the right element.
        Context.LabelId = Id ?? _labelId;
    }

    protected override SelectLabelState GetCurrentState() => new(Context.Disabled);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-disabled", Context.Disabled ? "" : null);
    }
}

public readonly record struct SelectLabelState(bool Disabled);
