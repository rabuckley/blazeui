using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.NumberField;

/// <summary>
/// Stepper button that decrements the number field value. Supports press-and-hold
/// repeat via JS pointer event listeners.
/// </summary>
public class NumberFieldDecrement : BlazeElement<NumberFieldButtonState>
{
    [CascadingParameter]
    internal NumberFieldContext Context { get; set; } = default!;

    private bool IsEffectivelyDisabled =>
        Context.Disabled || Context.ReadOnly ||
        (Context.Min.HasValue && Context.Value.HasValue && Context.Value.Value <= Context.Min.Value);

    // Guard against registering the press-and-hold JS listener more than once.
    // See NumberFieldIncrement for rationale.
    private bool _stepperRegistered;

    protected override string DefaultTag => "button";

    protected override NumberFieldButtonState GetCurrentState() => new(IsEffectivelyDisabled);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-disabled", IsEffectivelyDisabled ? "" : null);
        yield return new("data-readonly", Context.ReadOnly ? "" : null);
        yield return new("data-required", Context.Required ? "" : null);
    }

    private async Task HandleClick(MouseEventArgs _)
    {
        if (IsEffectivelyDisabled) return;
        await Context.StepValue(-1);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_stepperRegistered || Context.JsModule is null || Context.DotNetRef is null) return;

        _stepperRegistered = true;

        try
        {
            await Context.JsModule.InvokeVoidAsync("initStepper", ResolvedId, Context.DotNetRef, -1, Context.InstanceKey);
        }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddAttribute(1, "id", ResolvedId);

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

        builder.AddAttribute(6, "aria-label", "Decrease");
        builder.AddAttribute(7, "tabindex", "-1");

        if (IsEffectivelyDisabled)
            builder.AddAttribute(8, "disabled", true);

        builder.AddAttribute(9, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));

        builder.AddContent(10, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct NumberFieldButtonState(bool Disabled);
