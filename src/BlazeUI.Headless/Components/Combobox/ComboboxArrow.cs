using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Combobox;

public class ComboboxArrow : BlazeElement<ComboboxArrowState>
{
    [CascadingParameter] internal ComboboxContext Context { get; set; } = default!;
    protected override string DefaultTag => "div";
    protected override ComboboxArrowState GetCurrentState() => new(Context.Open);
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes() { yield break; }
}

public readonly record struct ComboboxArrowState(bool Open);
