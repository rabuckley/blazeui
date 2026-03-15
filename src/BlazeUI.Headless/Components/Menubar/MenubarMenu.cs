using BlazeUI.Headless.Components.Menu;
using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Menubar;

/// <summary>
/// Wraps a <see cref="MenuRoot"/> with controlled open state managed by the parent
/// <see cref="MenubarRoot"/>. Each instance represents one top-level menu in the bar.
/// </summary>
public class MenubarMenu : ComponentBase
{
    [CascadingParameter] internal MenubarContext Context { get; set; } = default!;

    /// <summary>
    /// Unique identifier for this menu within the menubar. Used for inter-menu navigation.
    /// </summary>
    [Parameter, EditorRequired] public string Value { get; set; } = "";

    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string _triggerId = "";

    protected override void OnInitialized()
    {
        _triggerId = IdGenerator.Next("menubar-trigger");
        Context.RegisterMenu(Value, _triggerId);
    }

    private bool IsOpen => Context.ActiveMenu == Value;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<MenuRoot>(0);
        builder.AddComponentParameter(1, "Open", IsOpen);
        builder.AddComponentParameter(2, "OpenChanged", EventCallback.Factory.Create<bool>(this, async (open) =>
        {
            if (open)
            {
                await Context.OpenMenu(Value);
                // Notify the menubar that a submenu is now open so it can set
                // data-has-submenu-open and enable hover-open on sibling triggers.
                Context.SetHasSubmenuOpen(true);
            }
            else if (Context.ActiveMenu == Value)
            {
                await Context.CloseAll();
                // Check whether any other menu is still open before clearing the flag.
                // ActiveMenu is nulled by CloseAll, so if nothing else is active the bar is idle.
                Context.SetHasSubmenuOpen(false);
            }
        }));
        builder.AddComponentParameter(3, "ChildContent", ChildContent);
        builder.CloseComponent();
    }
}
