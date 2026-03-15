using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.ContextMenu;

public class ContextMenuBackdrop : BlazeElement<ContextMenuBackdropState>
{
    [CascadingParameter] internal ContextMenuContext Context { get; set; } = default!;
    protected override string DefaultTag => "div";
    protected override ContextMenuBackdropState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-open", Context.Open ? "" : null);
        yield return new("data-closed", !Context.Open ? "" : null);
    }
}

public readonly record struct ContextMenuBackdropState(bool Open);
