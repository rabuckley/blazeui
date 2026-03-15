using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Field;

/// <summary>
/// The form control element for a field. Renders an <c>&lt;input&gt;</c> by default and
/// automatically wires up accessibility attributes (<c>id</c>, <c>name</c>,
/// <c>aria-invalid</c>, <c>aria-describedby</c>) from the parent <see cref="FieldRoot"/> context.
/// </summary>
public class FieldControl : BlazeElement<FieldControlState>
{
    [CascadingParameter]
    internal FieldContext? Context { get; set; }

    protected override string DefaultTag => "input";

    protected override FieldControlState GetCurrentState() => new(
        Context?.Disabled ?? false,
        Context?.Invalid ?? false,
        Context?.Dirty ?? false,
        Context?.Touched ?? false,
        Context?.Focused ?? false,
        Context?.Filled ?? false
    );

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        if (Context is null) yield break;

        yield return new("data-disabled", Context.Disabled ? "" : null);
        yield return new("data-valid", !Context.Invalid ? "" : null);
        yield return new("data-invalid", Context.Invalid ? "" : null);
        yield return new("data-dirty", Context.Dirty ? "" : null);
        yield return new("data-touched", Context.Touched ? "" : null);
        yield return new("data-filled", Context.Filled ? "" : null);
        yield return new("data-focused", Context.Focused ? "" : null);
    }

    protected override string ElementId => Context?.ControlId ?? ResolvedId;

    protected override void OnParametersSet()
    {
        // Propagate consumer Id into context so FieldLabel's `for` attribute
        // and other cross-references point at the right element.
        if (Id is not null && Context is not null)
            Context.ControlId = Id;
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        if (Context is null) yield break;

        // Wire the control to its label for assistive technologies.
        // The native <label for="…"> relationship handles mouse interaction; aria-labelledby
        // covers non-native label elements and programmatic access.
        if (Context.LabelId is not null)
            yield return new("aria-labelledby", Context.LabelId);

        // Wire the control to the label via aria-describedby (description + error).
        var describedBy = Context.GetDescribedBy();
        if (describedBy is not null)
            yield return new("aria-describedby", describedBy);

        // Signal invalid state to assistive technologies.
        if (Context.Invalid)
            yield return new("aria-invalid", "true");

        // Propagate the field name so the native form submission works correctly.
        if (Context.Name is not null)
            yield return new("name", Context.Name);
    }
}

/// <summary>
/// Snapshot of <see cref="FieldControl"/> state, passed to <c>ClassBuilder</c>
/// and <c>StyleBuilder</c> delegates.
/// </summary>
public readonly record struct FieldControlState(bool Disabled, bool Invalid, bool Dirty, bool Touched, bool Focused, bool Filled);
