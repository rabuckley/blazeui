namespace BlazeUI.Headless.Components.Autocomplete;

/// <summary>
/// Controls how the autocomplete filters items and whether it performs inline text completion.
/// </summary>
public enum AutocompleteMode
{
    /// <summary>
    /// Items are dynamically filtered based on the input value.
    /// The input value does not change based on the highlighted item.
    /// This is the default.
    /// </summary>
    List,

    /// <summary>
    /// Items are dynamically filtered based on the input value,
    /// which will temporarily change based on the highlighted item (inline autocompletion).
    /// </summary>
    Both,

    /// <summary>
    /// Items are not filtered. The input value will temporarily change based on the highlighted
    /// item (inline autocompletion).
    /// </summary>
    Inline,

    /// <summary>
    /// Items are not filtered. The input value does not change based on the highlighted item.
    /// </summary>
    None,
}
