using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.NumberField;

/// <summary>
/// Visual and semantic grouping wrapper for the input and stepper buttons.
/// Renders a <c>role="group"</c> container so assistive technologies announce
/// the increment/decrement buttons as belonging to the number field.
/// </summary>
public class NumberFieldGroup : BlazeElement<NumberFieldGroupState>
{
    [CascadingParameter]
    internal NumberFieldContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";

    protected override NumberFieldGroupState GetCurrentState() =>
        new(Context.Disabled, Context.ReadOnly, Context.Required, Context.IsScrubbing);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-disabled", Context.Disabled ? "" : null);
        yield return new("data-readonly", Context.ReadOnly ? "" : null);
        yield return new("data-required", Context.Required ? "" : null);
        yield return new("data-scrubbing", Context.IsScrubbing ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("role", "group");
    }
}

public readonly record struct NumberFieldGroupState(bool Disabled, bool ReadOnly, bool Required, bool Scrubbing);
