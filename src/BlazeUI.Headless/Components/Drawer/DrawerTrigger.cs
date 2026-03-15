using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Drawer;

public class DrawerTrigger : BlazeElement<DrawerTriggerState>
{
    [CascadingParameter] internal DrawerContext Context { get; set; } = default!;
    protected override string DefaultTag => "button";
    protected override DrawerTriggerState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-popup-open", Context.Open ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("aria-haspopup", "dialog");
        yield return new("aria-expanded", Context.Open ? "true" : "false");
        yield return new("onclick",
            EventCallback.Factory.Create<MouseEventArgs>(this, () => Context.SetOpen(!Context.Open)));
    }
}

public readonly record struct DrawerTriggerState(bool Open);
