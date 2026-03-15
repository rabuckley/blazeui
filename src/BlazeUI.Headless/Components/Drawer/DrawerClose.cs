using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Drawer;

public class DrawerClose : BlazeElement<DrawerCloseState>
{
    [CascadingParameter] internal DrawerContext Context { get; set; } = default!;
    protected override string DefaultTag => "button";
    protected override DrawerCloseState GetCurrentState() => default;
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes() { yield break; }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("onclick",
            EventCallback.Factory.Create<MouseEventArgs>(this, Context.Close));
    }
}

public readonly record struct DrawerCloseState;
