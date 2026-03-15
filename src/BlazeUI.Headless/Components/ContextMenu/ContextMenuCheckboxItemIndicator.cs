using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.ContextMenu;

public class ContextMenuCheckboxItemIndicator : BlazeElement<ContextMenuCheckboxItemIndicatorState>
{
    [Parameter] public bool Checked { get; set; }
    [Parameter] public bool KeepMounted { get; set; }

    protected override string DefaultTag => "span";
    protected override ContextMenuCheckboxItemIndicatorState GetCurrentState() => new(Checked);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-checked", Checked ? "" : null);
        yield return new("data-unchecked", !Checked ? "" : null);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!KeepMounted && !Checked) return;
        base.BuildRenderTree(builder);
    }
}

public readonly record struct ContextMenuCheckboxItemIndicatorState(bool Checked);
