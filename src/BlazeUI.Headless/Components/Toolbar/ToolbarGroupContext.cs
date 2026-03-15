namespace BlazeUI.Headless.Components.Toolbar;

/// <summary>
/// Cascaded by <see cref="ToolbarGroup"/> to communicate the group's disabled state to its children.
/// </summary>
internal sealed class ToolbarGroupContext
{
    public bool Disabled { get; set; }
}
