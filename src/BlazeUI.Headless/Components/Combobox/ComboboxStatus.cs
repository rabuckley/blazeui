using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Combobox;

/// <summary>
/// Displays a status message whose content changes are announced politely to screen readers.
/// Useful for conveying the status of an asynchronously loaded list.
/// Renders a <c>&lt;div role="status"&gt;</c> element. Matches Base UI's <c>Combobox.Status</c>.
/// </summary>
public class ComboboxStatus : BlazeElement<ComboboxStatusState>
{
    protected override string DefaultTag => "div";
    protected override ComboboxStatusState GetCurrentState() => default;
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes() { yield break; }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("role", "status");
        yield return new("aria-live", "polite");
        yield return new("aria-atomic", "true");
    }
}

public readonly record struct ComboboxStatusState;
