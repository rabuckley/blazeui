using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Switch;

public class SwitchThumb : BlazeElement<SwitchState>
{
    [CascadingParameter]
    internal SwitchContext Context { get; set; } = default!;

    protected override string DefaultTag => "span";

    protected override SwitchState GetCurrentState() =>
        new(Context.Checked, Context.Disabled, Context.ReadOnly, Context.Required);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-checked", Context.Checked ? "" : null);
        yield return new("data-unchecked", !Context.Checked ? "" : null);
        yield return new("data-disabled", Context.Disabled ? "" : null);
        yield return new("data-readonly", Context.ReadOnly ? "" : null);
        yield return new("data-required", Context.Required ? "" : null);
    }
}
