using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Field;

public class FieldDescription : BlazeElement<FieldState>
{
    [CascadingParameter]
    internal FieldContext? Context { get; set; }

    protected override string DefaultTag => "p";

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
        // Use the description ID from context so controls can reference it via aria-describedby
        builder.AddAttribute(1, "id", Context?.DescriptionId ?? ResolvedId);

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

        builder.AddContent(6, ChildContent);
        builder.CloseElement();
    }
}
