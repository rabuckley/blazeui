using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.ContextMenu;

public class ContextMenuGroupLabel : BlazeElement<ContextMenuGroupLabelState>
{
    protected override string DefaultTag => "div";
    protected override ContextMenuGroupLabelState GetCurrentState() => default;
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes() { yield break; }
}

public readonly record struct ContextMenuGroupLabelState;
