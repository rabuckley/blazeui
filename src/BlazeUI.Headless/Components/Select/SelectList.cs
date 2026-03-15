using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Select;

/// <summary>
/// Container inside the select popup that holds items. Provides <c>role="listbox"</c>
/// when used as a separate structural element (as opposed to the popup itself holding
/// the role). Registers its <c>id</c> in the context so other components can reference it.
/// </summary>
/// <remarks>
/// When <see cref="SelectList"/> is used, the <see cref="SelectPopup"/> should omit
/// its own <c>role="listbox"</c> and let the list provide it instead. For backward
/// compatibility, the popup still renders <c>role="listbox"</c> by default.
/// </remarks>
public class SelectList : BlazeElement<SelectListState>
{
    [CascadingParameter] internal SelectContext Context { get; set; } = default!;

    private string _listId = "";

    protected override string DefaultTag => "div";

    protected override void OnInitialized()
    {
        _listId = IdGenerator.Next("select-list");
        Context.ListId = _listId;
    }

    protected override string ElementId => _listId;

    protected override SelectListState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-open", Context.Open ? "" : null);
        yield return new("data-closed", !Context.Open ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("role", "listbox");
        yield return new("aria-labelledby", (object)(Context.LabelId ?? Context.TriggerId));
    }
}

public readonly record struct SelectListState(bool Open);
