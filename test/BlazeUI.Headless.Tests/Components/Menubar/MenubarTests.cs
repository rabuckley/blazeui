using BlazeUI.Headless.Components.Menubar;
using BlazeUI.Headless.Core;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Headless.Tests.Components.Menubar;

public class MenubarTests : BunitContext
{
    public MenubarTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazeUI();
    }

    // Renders a MenubarRoot containing a single MenubarMenu and MenubarTrigger.
    private IRenderedComponent<MenubarRoot> RenderMenubar(
        Action<RenderTreeBuilder>? menubarParams = null,
        Action<RenderTreeBuilder>? menuParams = null)
    {
        return Render<MenubarRoot>(builder =>
        {
            builder.OpenComponent<MenubarRoot>(0);
            menubarParams?.Invoke(builder);
            builder.AddComponentParameter(99, "ChildContent", (RenderFragment)(b =>
            {
                b.OpenComponent<MenubarMenu>(0);
                b.AddComponentParameter(1, "Value", "file");
                b.AddComponentParameter(2, "ChildContent", (RenderFragment)(menu =>
                {
                    menu.OpenComponent<MenubarTrigger>(0);
                    menu.AddComponentParameter(1, "ChildContent", (RenderFragment)(t =>
                        t.AddContent(0, "File")));
                    menu.CloseComponent();
                }));
                menuParams?.Invoke(b);
                b.CloseComponent();
            }));
            builder.CloseComponent();
        });
    }

    [Fact]
    public void Root_HasMenubarRole()
    {
        var cut = RenderMenubar();

        Assert.NotNull(cut.Find("[role='menubar']"));
    }

    [Fact]
    public void Root_RendersDiv()
    {
        var cut = RenderMenubar();

        Assert.Equal("DIV", cut.Find("div").TagName);
    }

    [Fact]
    public void Root_HasDefaultOrientationAttribute()
    {
        var cut = RenderMenubar();

        Assert.Equal("horizontal", cut.Find("[role='menubar']").GetAttribute("data-orientation"));
    }

    [Fact]
    public void Root_EmitsVerticalOrientationAttribute()
    {
        var cut = RenderMenubar(menubarParams: b =>
            b.AddComponentParameter(1, "Orientation", Orientation.Vertical));

        Assert.Equal("vertical", cut.Find("[role='menubar']").GetAttribute("data-orientation"));
    }

    [Fact]
    public void Root_HasSubmenuOpenInitiallyFalse()
    {
        var cut = RenderMenubar();

        // data-has-submenu-open is absent (null) when no submenu is open.
        Assert.Null(cut.Find("[role='menubar']").GetAttribute("data-has-submenu-open"));
    }

    [Fact]
    public void Trigger_HasCorrectAriaAttributes()
    {
        var cut = RenderMenubar();

        var trigger = cut.Find("button");
        Assert.Equal("menuitem", trigger.GetAttribute("role"));
        Assert.Equal("menu", trigger.GetAttribute("aria-haspopup"));
        Assert.Equal("false", trigger.GetAttribute("aria-expanded"));
    }


    [Fact]
    public void Trigger_NoPopupOpenAttributeWhenClosed()
    {
        var cut = RenderMenubar();

        // data-popup-open should be absent (null) when menu is closed.
        Assert.Null(cut.Find("button").GetAttribute("data-popup-open"));
    }

    [Fact]
    public void Trigger_DisabledWhenMenubarIsDisabled()
    {
        var cut = RenderMenubar(menubarParams: b =>
            b.AddComponentParameter(1, "Disabled", true));

        var trigger = cut.Find("button");
        Assert.NotNull(trigger.GetAttribute("disabled"));
        Assert.Equal("true", trigger.GetAttribute("aria-disabled"));
    }

    [Fact]
    public void Trigger_NotDisabledByDefault()
    {
        var cut = RenderMenubar();

        var trigger = cut.Find("button");
        Assert.Null(trigger.GetAttribute("disabled"));
        Assert.Null(trigger.GetAttribute("aria-disabled"));
    }
}
