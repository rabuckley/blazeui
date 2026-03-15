using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Radio;

/// <summary>
/// Visual indicator for the radio item. By default only renders when checked.
/// Must be placed inside a RadioRoot.
/// </summary>
public class RadioIndicator : BlazeElement<RadioState>
{
    [CascadingParameter]
    internal RadioRootContext Context { get; set; } = default!;

    /// <summary>
    /// When true, always renders even when unchecked.
    /// </summary>
    [Parameter]
    public bool KeepMounted { get; set; }

    protected override string DefaultTag => "span";

    protected override RadioState GetCurrentState() =>
        new(Context.Checked, Context.Disabled, Context.ReadOnly, Context.Required);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-checked", Context.Checked ? "" : null);
        yield return new("data-unchecked", !Context.Checked ? "" : null);
        yield return new("data-disabled", Context.Disabled ? "" : null);
        yield return new("data-readonly", Context.ReadOnly ? "" : null);
        yield return new("data-required", Context.Required ? "" : null);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Only render when checked, unless KeepMounted is set.
        if (!KeepMounted && !Context.Checked)
            return;

        base.BuildRenderTree(builder);
    }
}
