using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.ContextMenu;

/// <summary>
/// Nested state container for context menu submenus. Cascades its own context
/// for submenu open state.
/// </summary>
public class ContextMenuSubmenuRoot : ComponentBase
{
    [CascadingParameter] internal ContextMenuContext ParentContext { get; set; } = default!;

    [Parameter] public RenderFragment? ChildContent { get; set; }

    private readonly ComponentState<bool> _open = new(false);
    private readonly ContextMenuContext _context;

    public ContextMenuSubmenuRoot()
    {
        _context = new ContextMenuContext
        {
            PopupId = IdGenerator.Next("ctx-submenu-popup"),
            PositionerId = IdGenerator.Next("ctx-submenu-positioner"),
        };
        _context.SetOpen = SetOpenAsync;
        _context.Close = () => SetOpenAsync(false);
    }

    private async Task SetOpenAsync(bool value)
    {
        if (value)
        {
            // Close any sibling submenu before opening this one.
            if (ParentContext.CloseOpenSubmenu is not null)
                await ParentContext.CloseOpenSubmenu();

            ParentContext.CloseOpenSubmenu = () => SetOpenAsync(false);
        }
        else
        {
            ParentContext.CloseOpenSubmenu = null;
        }

        _open.SetInternal(value);
        _context.Open = _open.Value;
        StateHasChanged();
    }

    protected override void OnParametersSet()
    {
        // Inherit JS module from parent for positioning/animation.
        _context.JsModule = ParentContext.JsModule;
        _context.DotNetRef = ParentContext.DotNetRef;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<ContextMenuContext>>(0);
        builder.AddComponentParameter(1, "Value", _context);
        builder.AddComponentParameter(2, "ChildContent", ChildContent);
        builder.CloseComponent();
    }
}
