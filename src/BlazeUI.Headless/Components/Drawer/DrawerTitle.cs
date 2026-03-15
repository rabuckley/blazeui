using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Drawer;

public class DrawerTitle : BlazeElement<DrawerTitleState>
{
    [CascadingParameter] internal DrawerContext Context { get; set; } = default!;
    protected override string DefaultTag => "h2";
    protected override DrawerTitleState GetCurrentState() => default;
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes() { yield break; }
    protected override void OnInitialized() => Context.TitleId = ResolvedId;
}

public readonly record struct DrawerTitleState;
