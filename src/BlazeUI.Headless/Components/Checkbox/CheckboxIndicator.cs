using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Checkbox;

/// <summary>
/// Indicates whether the checkbox is ticked.
/// Only rendered (by default) when the checkbox is checked or indeterminate.
/// Set <see cref="KeepMounted"/> to keep it in the DOM at all times.
/// Renders a <c>&lt;span&gt;</c> element.
/// </summary>
public class CheckboxIndicator : BlazeElement<CheckboxState>
{
    [CascadingParameter]
    internal CheckboxContext Context { get; set; } = default!;

    /// <summary>
    /// When false (default), only renders when checked or indeterminate.
    /// When true, always renders regardless of checkbox state.
    /// </summary>
    [Parameter]
    public bool KeepMounted { get; set; }

    protected override string DefaultTag => "span";

    protected override CheckboxState GetCurrentState() =>
        new(Context.Checked, Context.Disabled, Context.Indeterminate, Context.ReadOnly, Context.Required);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        // Mirror the same state attribute set as CheckboxRoot so consumers can
        // target indicator styles using the same selectors.
        yield return new("data-checked", Context.Checked && !Context.Indeterminate ? "" : null);
        yield return new("data-unchecked", !Context.Checked && !Context.Indeterminate ? "" : null);
        yield return new("data-indeterminate", Context.Indeterminate ? "" : null);
        yield return new("data-disabled", Context.Disabled ? "" : null);
        yield return new("data-readonly", Context.ReadOnly ? "" : null);
        yield return new("data-required", Context.Required ? "" : null);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Only render when checked/indeterminate, unless KeepMounted
        if (!KeepMounted && !Context.Checked && !Context.Indeterminate)
            return;

        base.BuildRenderTree(builder);
    }
}
