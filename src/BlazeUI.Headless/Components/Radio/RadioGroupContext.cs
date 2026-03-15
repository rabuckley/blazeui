namespace BlazeUI.Headless.Components.Radio;

internal sealed class RadioGroupContext
{
    private readonly Func<string, bool> _isChecked;
    private readonly Func<string, Task> _select;

    public RadioGroupContext(
        Func<string, bool> isChecked,
        Func<string, Task> select,
        bool disabled,
        bool readOnly,
        bool required,
        string? name)
    {
        _isChecked = isChecked;
        _select = select;
        Disabled = disabled;
        ReadOnly = readOnly;
        Required = required;
        Name = name;
    }

    public bool IsChecked(string value) => _isChecked(value);
    public Task Select(string value) => _select(value);
    public bool Disabled { get; }
    public bool ReadOnly { get; }
    public bool Required { get; }
    public string? Name { get; }
}
