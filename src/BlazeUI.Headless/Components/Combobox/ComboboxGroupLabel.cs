using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Combobox;

/// <summary>
/// Accessible label for a <see cref="ComboboxGroup"/>. Automatically registers
/// its generated ID with the parent group so the group can emit
/// <c>aria-labelledby</c>.
/// </summary>
public class ComboboxGroupLabel : BlazeElement<ComboboxGroupLabelState>
{
    [CascadingParameter] internal ComboboxGroupContext? GroupContext { get; set; }

    private string? _labelId;

    protected override string DefaultTag => "div";

    protected override void OnInitialized()
    {
        _labelId = IdGenerator.Next("combobox-group-label");
        if (GroupContext is not null)
        {
            GroupContext.LabelId = _labelId;
            // Notify the parent group so it can re-render with aria-labelledby.
            GroupContext.OnLabelRegistered?.Invoke();
        }
    }

    protected override void OnParametersSet()
    {
        // Allow the consumer to override the ID, and keep the group in sync.
        if (Id is not null && GroupContext is not null && GroupContext.LabelId != Id)
        {
            GroupContext.LabelId = Id;
            GroupContext.OnLabelRegistered?.Invoke();
        }
    }

    protected override string ElementId => Id ?? _labelId ?? ResolvedId;

    protected override ComboboxGroupLabelState GetCurrentState() => default;
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes() { yield break; }
}

public readonly record struct ComboboxGroupLabelState;
