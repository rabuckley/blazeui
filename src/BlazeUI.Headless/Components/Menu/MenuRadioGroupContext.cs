namespace BlazeUI.Headless.Components.Menu;

/// <summary>
/// Internal context cascaded by <see cref="MenuRadioGroup"/> to its descendant
/// <see cref="MenuRadioItem"/> children so they can read and update the selected value
/// without coupling to <see cref="MenuContext"/>.
/// </summary>
internal sealed class MenuRadioGroupContext
{
    /// <summary>The currently selected value within the group.</summary>
    public string? Value { get; set; }

    /// <summary>Whether the entire group is disabled.</summary>
    public bool Disabled { get; set; }

    /// <summary>Called by a radio item when it is clicked to change the selection.</summary>
    public Func<string, Task> SetValue { get; set; } = _ => Task.CompletedTask;
}
