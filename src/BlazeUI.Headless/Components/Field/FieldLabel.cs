using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Field;

/// <summary>
/// The label element for a field. Renders a <c>&lt;label&gt;</c> by default and automatically
/// wires itself to the field control via the <c>for</c> attribute and registers its ID with
/// the <see cref="FieldContext"/> so that <see cref="FieldControl"/> can emit
/// <c>aria-labelledby</c>.
/// </summary>
public class FieldLabel : BlazeElement<FieldState>
{
    [CascadingParameter]
    internal FieldContext? Context { get; set; }

    protected override string DefaultTag => "label";

    protected override void OnParametersSet()
    {
        // Register the label's ID with the context so FieldControl can reference it via
        // aria-labelledby. This is the BlazeUI equivalent of Base UI's LabelableContext.
        if (Context is not null)
            Context.LabelId = ResolvedId;
    }

    protected override FieldState GetCurrentState() => new(
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

        // Wire the native label association — clicking the label focuses/activates the control.
        // A consumer-provided `for` in AdditionalAttributes (sequence 2) takes precedence.
        var hasExplicitFor = AdditionalAttributes?.ContainsKey("for") ?? false;
        if (Context is not null && !hasExplicitFor)
            builder.AddAttribute(6, "for", Context.ControlId);

        builder.AddContent(7, ChildContent);
        builder.CloseElement();
    }
}
