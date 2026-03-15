using BlazeUI.Headless.Components.Form;

namespace BlazeUI.Headless.Components.Field;

/// <summary>
/// Cascaded context from <see cref="FieldRoot"/> to child components (label, description, error, and controls).
/// Controls use this to wire up accessibility attributes and react to field state changes.
/// </summary>
internal sealed class FieldContext
{
    public string ControlId { get; set; } = "";
    public string? DescriptionId { get; set; }
    public string? ErrorId { get; set; }

    /// <summary>
    /// The ID of the rendered <see cref="FieldLabel"/> element. Set by <c>FieldLabel</c> during
    /// <c>OnParametersSet</c> so <see cref="FieldControl"/> can emit <c>aria-labelledby</c>.
    /// </summary>
    public string? LabelId { get; set; }

    public bool Invalid { get; set; }
    public bool Disabled { get; set; }
    public bool Dirty { get; set; }
    public bool Touched { get; set; }
    public bool Focused { get; set; }
    public bool Filled { get; set; }

    /// <summary>
    /// The field name from <see cref="FieldRoot.Name"/>. Used by child controls as the
    /// <c>name</c> attribute on the underlying form input.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The validation mode inherited from the parent form, or <c>OnSubmit</c> when
    /// there is no parent form. Individual fields may override this.
    /// </summary>
    public FormValidationMode ValidationMode { get; set; } = FormValidationMode.OnSubmit;

    /// <summary>
    /// Error messages for this field supplied by the parent form's <c>Errors</c> dictionary.
    /// Empty when the field has no parent form, no <c>Name</c>, or no matching error.
    /// </summary>
    public IReadOnlyList<string> FormErrors { get; set; } = [];

    /// <summary>
    /// Clears the form-level error for this field. Null when the field has no
    /// parent <c>FormRoot</c> or when the field has no <c>Name</c>.
    /// </summary>
    public Action? ClearFormErrors { get; set; }

    private readonly Action? _onChanged;

    public FieldContext(Action? onChanged = null)
    {
        _onChanged = onChanged;
    }

    public void SetFocused(bool focused)
    {
        Focused = focused;
        if (!Touched && !focused)
            Touched = true;
        _onChanged?.Invoke();
    }

    /// <summary>
    /// Builds the <c>aria-describedby</c> value for the field control by combining
    /// the description ID and (when invalid) the error ID.
    /// </summary>
    public string? GetDescribedBy()
    {
        var parts = new List<string>();
        if (DescriptionId is not null) parts.Add(DescriptionId);
        if (ErrorId is not null && Invalid) parts.Add(ErrorId);
        return parts.Count > 0 ? string.Join(' ', parts) : null;
    }
}
