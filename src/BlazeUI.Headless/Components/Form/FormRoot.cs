using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Form;

/// <summary>
/// A native form element with consolidated error handling.
/// Renders a <c>&lt;form&gt;</c> element with <c>novalidate</c> set by default,
/// delegating all validation to BlazeUI's field-level rules.
/// </summary>
public class FormRoot : BlazeElement<FormState>
{
    /// <summary>
    /// Determines when child fields validate their values.
    /// Individual <c>Field.Root</c> components may override this per-field.
    ///
    /// <list type="bullet">
    ///   <item><description><c>OnSubmit</c> (default): validates on submit; re-validates on change after the first attempt.</description></item>
    ///   <item><description><c>OnBlur</c>: validates when a control loses focus.</description></item>
    ///   <item><description><c>OnChange</c>: validates on every value change.</description></item>
    /// </list>
    /// </summary>
    [Parameter]
    public FormValidationMode ValidationMode { get; set; } = FormValidationMode.OnSubmit;

    /// <summary>
    /// Validation errors returned externally, typically from a server action or API call.
    /// Keys correspond to the <c>Name</c> attribute on <c>Field.Root</c>; values are
    /// one or more error messages for that field.
    /// </summary>
    [Parameter]
    public IReadOnlyDictionary<string, IReadOnlyList<string>>? Errors { get; set; }

    /// <summary>
    /// Disables browser-native constraint validation. Defaults to <c>true</c> so that
    /// BlazeUI's own validation logic handles all feedback. Set to <c>false</c> to
    /// opt back in to native browser validation bubbles.
    /// </summary>
    [Parameter]
    public bool NoValidate { get; set; } = true;

    // The cascaded context mutated in OnParametersSet and consumed by child Field components.
    private FormContext? _context;

    protected override string DefaultTag => "form";

    protected override FormState GetCurrentState() => default;

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield break;
    }

    protected override void OnInitialized()
    {
        // ClearErrors is a delegate that removes a single field's error entry from the
        // mutable snapshot so the field doesn't remain visually invalid after the user
        // starts correcting their input.
        _context = new FormContext
        {
            ClearErrors = ClearFieldError,
        };
    }

    protected override void OnParametersSet()
    {
        if (_context is null) return;

        _context.ValidationMode = ValidationMode;

        // Snapshot the external errors dictionary on every render so child fields see
        // up-to-date server errors when the parent re-renders with new values.
        _context.Errors = Errors ?? new Dictionary<string, IReadOnlyList<string>>();
    }

    private void ClearFieldError(string? fieldName)
    {
        if (_context is null || fieldName is null) return;
        if (!_context.Errors.ContainsKey(fieldName)) return;

        // Build a new dictionary without the cleared field so the reference changes
        // and cascading consumers receive the updated value.
        var next = new Dictionary<string, IReadOnlyList<string>>(_context.Errors);
        next.Remove(fieldName);
        _context.Errors = next;

        // Notify Blazor to re-render this subtree.
        InvokeAsync(StateHasChanged);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddMultipleAttributes(1, BuildAttributes(state));

        if (NoValidate)
            builder.AddAttribute(2, "novalidate", true);

        // Cascade context so all nested Field components can read form-level
        // errors and the active validation mode without prop drilling.
        builder.OpenComponent<CascadingValue<FormContext>>(3);
        builder.AddComponentParameter(4, "Value", _context);
        builder.AddComponentParameter(5, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();
    }
}

/// <summary>The state record for <see cref="FormRoot"/>. Currently empty; reserved for future state attributes.</summary>
public readonly record struct FormState;

/// <summary>
/// Controls when fields within a <see cref="FormRoot"/> run validation.
/// </summary>
public enum FormValidationMode
{
    /// <summary>Validates on submit; re-validates on change after the first submission attempt.</summary>
    OnSubmit,

    /// <summary>Validates when a control loses focus.</summary>
    OnBlur,

    /// <summary>Validates on every change to the control value.</summary>
    OnChange,
}
