namespace BlazeUI.Headless.Components.Menu;

/// <summary>
/// Internal context cascaded by <see cref="MenuGroup"/> so that a child
/// <see cref="MenuGroupLabel"/> can register its generated ID, which the group
/// then wires up as its <c>aria-labelledby</c> attribute.
/// </summary>
internal sealed class MenuGroupContext
{
    public string? LabelId { get; set; }

    /// <summary>Called by <see cref="MenuGroupLabel"/> to push its resolved ID.</summary>
    public Action<string?> SetLabelId { get; set; } = _ => { };
}
