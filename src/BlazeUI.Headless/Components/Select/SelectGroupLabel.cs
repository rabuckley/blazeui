using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Select;

/// <summary>
/// An accessible label that is automatically associated with its parent group.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
public class SelectGroupLabel : BlazeElement<SelectGroupLabelState>
{
    [CascadingParameter] internal SelectGroupContext GroupContext { get; set; } = default!;

    private string _labelId = "";

    protected override string DefaultTag => "div";

    protected override void OnInitialized()
    {
        _labelId = IdGenerator.Next("select-group-label");

        // Notify the parent SelectGroup so it can update aria-labelledby via StateHasChanged.
        if (GroupContext is not null)
            GroupContext.SetLabelId(_labelId);
    }

    protected override string ElementId => Id ?? _labelId;

    protected override void OnParametersSet()
    {
        // Propagate effective ID into context so the group's
        // aria-labelledby points at the right element. Guard against
        // redundant updates — SetLabelId triggers a group re-render which
        // would re-cascade parameters and cause an infinite loop.
        var effectiveId = Id ?? _labelId;
        if (GroupContext is not null && GroupContext.LabelId != effectiveId)
            GroupContext.SetLabelId(effectiveId);
    }

    protected override SelectGroupLabelState GetCurrentState() => default;
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes() { yield break; }
}

public readonly record struct SelectGroupLabelState;
