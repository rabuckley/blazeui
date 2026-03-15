namespace BlazeUI.Headless.Components.Select;

/// <summary>
/// Cascaded by <see cref="SelectGroup"/> to <see cref="SelectGroupLabel"/> so the label
/// can register its generated <c>id</c> for the group's <c>aria-labelledby</c>.
/// </summary>
internal sealed class SelectGroupContext
{
    public string? LabelId { get; set; }
    public Action<string>? SetLabelId { get; set; }
}
