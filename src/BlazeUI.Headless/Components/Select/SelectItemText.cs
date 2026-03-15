using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Select;

/// <summary>
/// A text label of the select item.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
public class SelectItemText : BlazeElement<SelectItemTextState>
{
    protected override string DefaultTag => "div";
    protected override SelectItemTextState GetCurrentState() => default;
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes() { yield break; }
}

public readonly record struct SelectItemTextState;
