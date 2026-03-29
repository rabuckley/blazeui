using BlazeUI.Headless.Components.Field;
using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Input;

/// <summary>
/// A native input element that automatically wires up accessibility attributes
/// and validation state when nested inside a <see cref="FieldRoot"/>.
/// Renders an <c>&lt;input&gt;</c> element.
/// Supports two-way binding via <see cref="Value"/>/<see cref="ValueChanged"/>,
/// or uncontrolled mode via <see cref="DefaultValue"/>.
/// When none of these are set, <c>value</c> and <c>oninput</c> can still be
/// passed through <c>AdditionalAttributes</c> for manual wiring.
/// </summary>
public class Input : BlazeElement<InputState>
{
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// The current value of the input (controlled mode). Use <c>@bind-Value</c>
    /// for two-way binding. The component never mutates this parameter directly —
    /// it fires <see cref="ValueChanged"/> on user input.
    /// </summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// Initial value for uncontrolled mode. Ignored when <see cref="Value"/> is set.
    /// </summary>
    [Parameter]
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Callback invoked on each <c>oninput</c> event with the new string value.
    /// </summary>
    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    [CascadingParameter]
    internal FieldContext? FieldContext { get; set; }

    private readonly ComponentState<string?> _value = new(null);

    // True when Value, DefaultValue, or ValueChanged is actively used.
    // When false, value/oninput pass through AdditionalAttributes untouched.
    private bool _managingValue;

    // Local focus tracking so the component works standalone (no FieldContext).
    private bool _focused;

    protected override string DefaultTag => "input";

    protected override void OnInitialized()
    {
        if (DefaultValue is not null)
            _value.SetInternal(DefaultValue);
    }

    protected override void OnParametersSet()
    {
        // Propagate consumer Id into context so FieldLabel's `for` attribute
        // and other cross-references point at the right element.
        if (Id is not null && FieldContext is not null)
            FieldContext.ControlId = Id;

        _managingValue = Value is not null || DefaultValue is not null || ValueChanged.HasDelegate;

        if (Value is not null)
            _value.SetControlled(Value);
        else
            _value.ClearControlled();
    }

    // When inside a Field, pull all state from the context so that Field-driven
    // mutations (touched, dirty, filled, invalid) are reflected immediately.
    protected override InputState GetCurrentState()
    {
        if (FieldContext is not null)
        {
            return new(
                Disabled: FieldContext.Disabled,
                Focused: FieldContext.Focused,
                Valid: !FieldContext.Invalid,
                Invalid: FieldContext.Invalid,
                Dirty: FieldContext.Dirty,
                Touched: FieldContext.Touched,
                Filled: FieldContext.Filled
            );
        }

        return new(Disabled: Disabled, Focused: _focused, Valid: false, Invalid: false, Dirty: false, Touched: false, Filled: false);
    }

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        var state = GetCurrentState();

        yield return new("data-disabled", state.Disabled ? "" : null);

        // Validity attributes are only meaningful inside a Field. Without Field
        // context there is no validation to report, so we omit them.
        if (FieldContext is not null)
        {
            yield return new("data-valid", state.Valid ? "" : null);
            yield return new("data-invalid", state.Invalid ? "" : null);
            yield return new("data-dirty", state.Dirty ? "" : null);
            yield return new("data-touched", state.Touched ? "" : null);
            yield return new("data-filled", state.Filled ? "" : null);
        }

        yield return new("data-focused", state.Focused ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        // The id must match the Field's control slot so that the associated
        // <label for="..."> and aria-describedby point at this element.
        yield return new("id", FieldContext?.ControlId ?? ElementId);

        if (Disabled || (FieldContext?.Disabled ?? false))
            yield return new("disabled", (object)true);

        // Only emit value/oninput when the consumer opted into managed value.
        // Otherwise, value and oninput can be supplied through AdditionalAttributes.
        if (_managingValue)
        {
            yield return new("value", (object?)_value.Value ?? "");
            yield return new("oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, HandleInput));
        }

        yield return new("onfocus", EventCallback.Factory.Create<FocusEventArgs>(this, HandleFocus));
        yield return new("onblur", EventCallback.Factory.Create<FocusEventArgs>(this, HandleBlur));

        if (FieldContext is not null)
        {
            var describedBy = FieldContext.GetDescribedBy();
            if (!string.IsNullOrEmpty(describedBy))
                yield return new("aria-describedby", describedBy);

            if (FieldContext.Invalid)
                yield return new("aria-invalid", (object)"true");
        }
    }

    private async Task HandleInput(ChangeEventArgs e)
    {
        var newValue = e.Value?.ToString();
        _value.SetInternal(newValue);

        if (ValueChanged.HasDelegate)
            await ValueChanged.InvokeAsync(newValue);
    }

    private void HandleFocus(FocusEventArgs _)
    {
        _focused = true;
        FieldContext?.SetFocused(true);
    }

    private void HandleBlur(FocusEventArgs _)
    {
        _focused = false;
        FieldContext?.SetFocused(false);
    }
}

/// <summary>
/// The current state of an <see cref="Input"/>, exposed to <c>ClassBuilder</c> and <c>StyleBuilder</c>.
/// </summary>
public readonly record struct InputState(
    bool Disabled,
    bool Focused,
    bool Valid,
    bool Invalid,
    bool Dirty,
    bool Touched,
    bool Filled);
