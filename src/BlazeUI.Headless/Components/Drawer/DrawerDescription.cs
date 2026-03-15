using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Drawer;

public class DrawerDescription : BlazeElement<DrawerDescriptionState>
{
    [CascadingParameter] internal DrawerContext Context { get; set; } = default!;
    protected override string DefaultTag => "p";
    protected override DrawerDescriptionState GetCurrentState() => default;
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes() { yield break; }
    protected override void OnInitialized() => Context.DescriptionId = ResolvedId;
}

public readonly record struct DrawerDescriptionState;
