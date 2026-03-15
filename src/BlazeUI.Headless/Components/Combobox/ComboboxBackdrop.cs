using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Combobox;

/// <summary>
/// An overlay displayed beneath the combobox popup.
/// Matches Base UI's <c>Combobox.Backdrop</c>.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
public class ComboboxBackdrop : BlazeElement<ComboboxBackdropState>
{
    [CascadingParameter] internal ComboboxContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";
    protected override ComboboxBackdropState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-open", Context.Open ? "" : null);
        yield return new("data-closed", !Context.Open ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("role", "presentation");
    }
}

public readonly record struct ComboboxBackdropState(bool Open);
