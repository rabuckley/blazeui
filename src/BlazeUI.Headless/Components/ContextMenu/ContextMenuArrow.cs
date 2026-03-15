using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.ContextMenu;

public class ContextMenuArrow : BlazeElement<ContextMenuArrowState>
{
    [CascadingParameter] internal ContextMenuContext Context { get; set; } = default!;
    protected override string DefaultTag => "div";
    protected override ContextMenuArrowState GetCurrentState() => new(Context.Open);
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes() { yield break; }
}

public readonly record struct ContextMenuArrowState(bool Open);
