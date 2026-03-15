using System.Globalization;
using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.NumberField;

public class NumberFieldInput : BlazeElement<NumberFieldInputState>
{
    [CascadingParameter]
    internal NumberFieldContext Context { get; set; } = default!;

    protected override string DefaultTag => "input";

    protected override NumberFieldInputState GetCurrentState() =>
        new(Context.Value, Context.Disabled, Context.ReadOnly, Context.Required);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-disabled", Context.Disabled ? "" : null);
        yield return new("data-readonly", Context.ReadOnly ? "" : null);
        yield return new("data-required", Context.Required ? "" : null);
    }

    private async Task HandleInput(ChangeEventArgs e)
    {
        var raw = e.Value?.ToString();
        if (string.IsNullOrWhiteSpace(raw))
        {
            await Context.SetValue(null);
            return;
        }

        if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
        {
            await Context.SetValue(parsed);
        }
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (Context.Disabled || Context.ReadOnly) return;

        switch (e.Key)
        {
            case "ArrowUp":
                // Shift = large step, Alt = small step
                if (e.ShiftKey)
                    await StepByLarge(1);
                else if (e.AltKey)
                    await StepBySmall(1);
                else
                    await Context.StepValue(1);
                break;

            case "ArrowDown":
                if (e.ShiftKey)
                    await StepByLarge(-1);
                else if (e.AltKey)
                    await StepBySmall(-1);
                else
                    await Context.StepValue(-1);
                break;

            case "Home":
                // Set to minimum when min is defined.
                if (Context.Min.HasValue)
                    await Context.SetValue(Context.Min.Value);
                break;

            case "End":
                // Set to maximum when max is defined.
                if (Context.Max.HasValue)
                    await Context.SetValue(Context.Max.Value);
                break;
        }
    }

    private async Task StepByLarge(int sign)
    {
        // Large step is applied as a direct offset rather than N discrete steps so
        // that non-integer large steps (e.g. largeStep=10, step=3) behave correctly.
        var current = Context.Value ?? 0;
        await Context.SetValue(current + sign * Context.LargeStep);
    }

    private async Task StepBySmall(int sign)
    {
        var current = Context.Value ?? 0;
        await Context.SetValue(current + sign * Context.SmallStep);
    }

    /// <summary>
    /// Clamp value to min/max when focus leaves the input.
    /// </summary>
    private async Task HandleBlur(FocusEventArgs _)
    {
        if (Context.Value.HasValue)
        {
            // Re-set the value to trigger clamping in the root.
            await Context.SetValue(Context.Value);
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddAttribute(1, "id", Context.InputId);

        if (AdditionalAttributes is not null)
            builder.AddMultipleAttributes(2, AdditionalAttributes);

        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass))
            builder.AddAttribute(3, "class", mergedClass);

        var mergedStyle = Css.Cn(Style, StyleBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedStyle))
            builder.AddAttribute(4, "style", mergedStyle);

        foreach (var attr in GetDataAttributes())
            if (attr.Value is not null)
                builder.AddAttribute(5, attr.Key, attr.Value);

        builder.AddAttribute(6, "inputmode", "decimal");
        builder.AddAttribute(7, "aria-roledescription", "Number field");
        builder.AddAttribute(8, "value", Context.Value?.ToString(CultureInfo.InvariantCulture));

        if (Context.Disabled)
            builder.AddAttribute(9, "disabled", true);
        if (Context.ReadOnly)
            builder.AddAttribute(10, "readonly", true);
        if (Context.Required)
            builder.AddAttribute(11, "required", true);

        builder.AddAttribute(12, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, HandleInput));
        builder.AddAttribute(13, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDown));
        builder.AddAttribute(14, "onblur", EventCallback.Factory.Create<FocusEventArgs>(this, HandleBlur));

        builder.AddContent(15, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct NumberFieldInputState(double? Value, bool Disabled, bool ReadOnly, bool Required);
