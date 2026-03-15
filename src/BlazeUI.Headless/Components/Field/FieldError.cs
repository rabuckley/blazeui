using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Field;

public class FieldError : BlazeElement<FieldState>
{
    [CascadingParameter]
    internal FieldContext? Context { get; set; }

    /// <summary>
    /// Force rendering even when there are no errors (when KeepMounted is true).
    /// </summary>
    [Parameter]
    public bool KeepMounted { get; set; }

    protected override string DefaultTag => "div";

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
        // Only render when the field is invalid, unless KeepMounted keeps it in the DOM.
        if (!KeepMounted && !(Context?.Invalid ?? false))
            return;

        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddAttribute(1, "id", Context?.ErrorId ?? ResolvedId);

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

        builder.AddAttribute(6, "aria-live", "polite");

        // When form-level error messages are present (e.g. from a server action),
        // render them automatically. Explicit ChildContent overrides them so
        // consumers can supply custom markup if needed.
        var formErrors = Context?.FormErrors;
        if (ChildContent is not null)
        {
            builder.AddContent(7, ChildContent);
        }
        else if (formErrors is { Count: > 0 })
        {
            // Render all error messages joined by a space, matching the single-string
            // display Base UI uses when the errors prop contains a single string per field.
            builder.AddContent(7, string.Join(" ", formErrors));
        }

        builder.CloseElement();
    }
}
