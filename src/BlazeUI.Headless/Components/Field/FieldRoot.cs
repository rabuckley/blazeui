using BlazeUI.Headless.Components.Form;
using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Field;

public class FieldRoot : BlazeElement<FieldState>
{
    /// <summary>
    /// Identifies the field when looked up in the parent form's <c>Errors</c> dictionary.
    /// Should match the <c>name</c> attribute on the inner form control so that server-side
    /// errors returned by form actions can be mapped back to the correct field.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Explicitly marks the field as invalid regardless of validation results.
    /// When omitted, validity is derived from form-level errors (via <c>Name</c>)
    /// and any client-side validation logic.
    /// </summary>
    [Parameter]
    public bool? Invalid { get; set; }

    // Form context cascaded from FormRoot. Optional — fields can be used standalone.
    [CascadingParameter]
    internal FormContext? FormContext { get; set; }

    private FieldContext? _context;

    protected override string DefaultTag => "div";

    protected override FieldState GetCurrentState() => new(
        _context?.Disabled ?? false,
        _context?.Invalid ?? false,
        _context?.Dirty ?? false,
        _context?.Touched ?? false,
        _context?.Focused ?? false,
        _context?.Filled ?? false
    );

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        var ctx = _context;
        if (ctx is null) yield break;

        yield return new("data-disabled", ctx.Disabled ? "" : null);
        yield return new("data-valid", !ctx.Invalid ? "" : null);
        yield return new("data-invalid", ctx.Invalid ? "" : null);
        yield return new("data-dirty", ctx.Dirty ? "" : null);
        yield return new("data-touched", ctx.Touched ? "" : null);
        yield return new("data-filled", ctx.Filled ? "" : null);
        yield return new("data-focused", ctx.Focused ? "" : null);
    }

    protected override void OnInitialized()
    {
        _context = new FieldContext(() => InvokeAsync(StateHasChanged))
        {
            ControlId = IdGenerator.Next("field-control"),
            DescriptionId = IdGenerator.Next("field-description"),
            ErrorId = IdGenerator.Next("field-error")
        };
    }

    protected override void OnParametersSet()
    {
        if (_context is null) return;

        _context.Disabled = Disabled;
        _context.Name = Name;

        // Determine whether the field is invalid. Explicit `Invalid` override takes
        // precedence; otherwise check for a matching server error in the form context.
        if (Invalid.HasValue)
        {
            _context.Invalid = Invalid.Value;
        }
        else if (FormContext is not null && Name is not null)
        {
            // A form error for this field name marks the field as invalid even before
            // the user has interacted with it, mirroring Base UI's behaviour.
            _context.Invalid = FormContext.Errors.ContainsKey(Name);
        }
        else
        {
            _context.Invalid = false;
        }

        // Propagate the form's ValidationMode to the field context so child controls
        // can adapt their change-handling logic without reading the form context directly.
        if (FormContext is not null)
            _context.ValidationMode = FormContext.ValidationMode;

        // Expose the ClearErrors delegate so FieldControl and similar children can
        // notify the form when the user starts correcting an erroneous field.
        if (FormContext is not null)
        {
            _context.ClearFormErrors = () => FormContext.ClearErrors(Name);

            // Copy form-level error messages into the field context so FieldError
            // can display them without reading the form context directly.
            _context.FormErrors = FormContext.GetErrors(Name);
        }
        else
        {
            _context.FormErrors = [];
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddMultipleAttributes(1, BuildAttributes(state));

        // Cascade context to children
        builder.OpenComponent<CascadingValue<FieldContext>>(2);
        builder.AddComponentParameter(3, "Value", _context);
        builder.AddComponentParameter(4, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();
    }
}

public readonly record struct FieldState(
    bool Disabled, bool Invalid, bool Dirty, bool Touched, bool Focused, bool Filled);
