namespace BlazeUI.Headless.Components.Form;

/// <summary>
/// Cascaded context from <see cref="FormRoot"/> to child <c>Field</c> components.
/// Fields read this to apply form-level errors and the active validation mode.
/// </summary>
internal sealed class FormContext
{
    /// <summary>
    /// Server-returned or externally-supplied errors, keyed by field name.
    /// Each value is one or more error message strings.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Errors { get; set; } =
        new Dictionary<string, IReadOnlyList<string>>();

    /// <summary>
    /// When and how fields should run their validation logic.
    /// Fields may override this per-field; the form-level value is the default.
    /// </summary>
    public FormValidationMode ValidationMode { get; set; } = FormValidationMode.OnSubmit;

    /// <summary>
    /// True once the user has attempted to submit the form at least once.
    /// Fields in <c>onSubmit</c> mode re-validate on every change after the first attempt.
    /// </summary>
    public bool SubmitAttempted { get; set; }

    /// <summary>
    /// Removes the error entry for a specific field name. Called by fields when their
    /// value changes so stale server errors are cleared without requiring a re-submit.
    /// </summary>
    public Action<string?> ClearErrors { get; set; } = _ => { };

    /// <summary>
    /// Returns the error messages for the given field name, or an empty list when there
    /// are none. Callers can display the first element or join all messages.
    /// </summary>
    public IReadOnlyList<string> GetErrors(string? fieldName)
    {
        if (fieldName is null || !Errors.TryGetValue(fieldName, out var errors))
            return [];

        return errors;
    }
}
