using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Combobox;

/// <summary>
/// Scrollable listbox container inside the combobox popup.
/// Renders <c>role="listbox"</c> and is the target of the input's
/// <c>aria-controls</c>. Matches Base UI's <c>Combobox.List</c>.
/// </summary>
public class ComboboxList : BlazeElement<ComboboxListState>
{
    [CascadingParameter] internal ComboboxContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";
    protected override string ElementId => Context.ListId;
    protected override ComboboxListState GetCurrentState() => new(Context.Open, Context.Multiple);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-empty", Context.IsEmpty ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("role", "listbox");
        yield return new("tabindex", "-1");
        // aria-multiselectable is set when multiple selection is enabled, matching Base UI.
        if (Context.Multiple)
            yield return new("aria-multiselectable", "true");
    }
}

public readonly record struct ComboboxListState(bool Open, bool Multiple);
