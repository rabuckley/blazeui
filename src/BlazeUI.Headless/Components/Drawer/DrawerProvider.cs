using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Drawer;

/// <summary>
/// Coordinates global drawer state across the app. Enables indent/background
/// effects when nested drawers are open. Wrap the app's main content and drawer
/// instances in a single <c>DrawerProvider</c>.
/// </summary>
public class DrawerProvider : ComponentBase
{
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private readonly DrawerProviderContext _context = new();

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<DrawerProviderContext>>(0);
        builder.AddComponentParameter(1, "Value", _context);
        builder.AddComponentParameter(2, "ChildContent", ChildContent);
        builder.CloseComponent();
    }
}
