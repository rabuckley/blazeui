using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Select;

/// <summary>
/// A button that opens the select popup.
/// Renders a <c>&lt;button&gt;</c> element.
/// </summary>
public class SelectTrigger : BlazeElement<SelectTriggerState>
{
    [CascadingParameter] internal SelectContext Context { get; set; } = default!;

    protected override string DefaultTag => "button";
    protected override string ElementId => Id ?? Context.TriggerId;

    protected override void OnParametersSet()
    {
        // Propagate consumer Id into context so popup positioning and
        // aria-labelledby references stay consistent.
        if (Id is not null)
            Context.TriggerId = Id;
    }

    protected override SelectTriggerState GetCurrentState() =>
        new(Context.Open, Context.Disabled, Context.ReadOnly, Context.Required);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-popup-open", Context.Open ? "" : null);
        yield return new("data-disabled", Context.Disabled ? "" : null);
        yield return new("data-readonly", Context.ReadOnly ? "" : null);
        yield return new("data-required", Context.Required ? "" : null);
        yield return new("data-placeholder", Context.SelectedValue is null ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("role", "combobox");
        yield return new("aria-haspopup", "listbox");
        yield return new("aria-expanded", Context.Open ? "true" : "false");

        if (Context.LabelId is not null)
            yield return new("aria-labelledby", Context.LabelId);

        if (Context.PopupId is not null)
            yield return new("aria-controls", Context.PopupId);

        if (Context.Disabled)
            yield return new("disabled", true);

        yield return new("onclick",
            EventCallback.Factory.Create<MouseEventArgs>(this, () => Context.SetOpen(!Context.Open)));
    }
}

public readonly record struct SelectTriggerState(bool Open, bool Disabled, bool ReadOnly, bool Required);
