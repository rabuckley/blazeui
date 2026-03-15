using BlazeUI.Headless.Components.NavigationMenu;
using BlazeUI.Headless.Core;
using Bunit;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Headless.Tests.Components.NavigationMenu;

public class NavigationMenuTests : BunitContext
{
    public NavigationMenuTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazeUI();
    }

    [Fact]
    public void Root_RendersNavElement()
    {
        var cut = RenderNavigationMenu();
        var nav = cut.Find("nav");
        Assert.Equal("NAV", nav.TagName);
    }

    [Fact]
    public void Root_HasHorizontalOrientationByDefault()
    {
        var cut = RenderNavigationMenu();
        var nav = cut.Find("nav");
        Assert.Equal("horizontal", nav.GetAttribute("data-orientation"));
    }

    [Fact]
    public void List_RendersUlByDefault()
    {
        var cut = RenderNavigationMenu();
        var list = cut.Find("ul");
        Assert.Equal("UL", list.TagName);
    }

    [Fact]
    public void List_DoesNotEmitDataOrientation()
    {
        // Base UI explicitly omits data-orientation from the list element;
        // orientation is conveyed by the root <nav>'s data-orientation instead.
        var cut = RenderNavigationMenu();
        Assert.Null(cut.Find("ul").GetAttribute("data-orientation"));
    }

    [Fact]
    public void Item_RendersLiByDefault()
    {
        var cut = RenderNavigationMenu();
        var items = cut.FindAll("li");
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public void Trigger_HasAriaExpandedFalse()
    {
        var cut = RenderNavigationMenu();
        var triggers = cut.FindAll("button");
        Assert.All(triggers, t => Assert.Equal("false", t.GetAttribute("aria-expanded")));
    }

    [Fact]
    public void Trigger_HasBaseUiIdentifierAttribute()
    {
        // Base UI marks all triggers with this attribute for dismiss/hover detection.
        var cut = RenderNavigationMenu();
        var triggers = cut.FindAll("button");
        Assert.All(triggers, t => t.HasAttribute("data-base-ui-navigation-menu-trigger"));
    }

    [Fact]
    public void Trigger_HasNoDataPopupOpenWhenClosed()
    {
        var cut = RenderNavigationMenu();
        var trigger = cut.FindAll("button")[0];
        Assert.False(trigger.HasAttribute("data-popup-open"));
        Assert.False(trigger.HasAttribute("data-pressed"));
    }

    [Fact]
    public void Trigger_HasDataPopupOpenAndDataPressedWhenOpen()
    {
        var cut = RenderNavigationMenu();
        cut.FindAll("button")[0].Click();
        var trigger = cut.FindAll("button")[0];
        Assert.True(trigger.HasAttribute("data-popup-open"));
        Assert.True(trigger.HasAttribute("data-pressed"));
    }

    [Fact]
    public void Trigger_HasAriaExpandedTrueWhenOpen()
    {
        var cut = RenderNavigationMenu();
        cut.FindAll("button")[0].Click();
        Assert.Equal("true", cut.FindAll("button")[0].GetAttribute("aria-expanded"));
    }

    [Fact]
    public void Trigger_ClickTogglesActive()
    {
        string? received = null;
        var cut = RenderNavigationMenu(onValueChanged: v => received = v);

        var trigger = cut.FindAll("button")[0];
        trigger.Click();

        Assert.Equal("products", received);
    }

    [Fact]
    public void Link_RendersAsAnchor()
    {
        var cut = RenderWithLink();
        var link = cut.Find("a");
        Assert.Equal("A", link.TagName);
    }

    [Fact]
    public void Link_HasAriaCurrentPageWhenActive()
    {
        var cut = RenderWithLink(active: true);
        Assert.Equal("page", cut.Find("a").GetAttribute("aria-current"));
    }

    [Fact]
    public void Link_HasNoAriaCurrentWhenInactive()
    {
        var cut = RenderWithLink(active: false);
        Assert.Null(cut.Find("a").GetAttribute("aria-current"));
    }

    [Fact]
    public void Link_ClosesMenuOnClick_WhenCloseOnClickIsTrue()
    {
        // CloseOnClick defaults to false (Base UI default). Explicitly opt in to test the
        // close-on-click behaviour.
        string? received = null;
        var cut = RenderNavigationMenuWithContent(onValueChanged: v => received = v);

        // Open the menu first
        cut.Find("button").Click();
        Assert.Equal("products", received);

        // Clicking a link with CloseOnClick=true should close the menu.
        cut.Find("a").Click();
        Assert.Null(received);
    }

    private Bunit.IRenderedComponent<Bunit.Rendering.ContainerFragment> RenderNavigationMenu(
        Action<string?>? onValueChanged = null)
    {
        return Render(builder =>
        {
            builder.OpenComponent<NavigationMenuRoot>(0);
            if (onValueChanged is not null)
                builder.AddComponentParameter(1, "ValueChanged",
                    Microsoft.AspNetCore.Components.EventCallback.Factory.Create<string?>(this, onValueChanged));
            builder.AddComponentParameter(2, "ChildContent",
                (Microsoft.AspNetCore.Components.RenderFragment)(b =>
                {
                    b.OpenComponent<NavigationMenuList>(0);
                    b.AddComponentParameter(1, "ChildContent",
                        (Microsoft.AspNetCore.Components.RenderFragment)(list =>
                        {
                            list.OpenComponent<NavigationMenuItem>(0);
                            list.AddComponentParameter(1, "Value", "products");
                            list.AddComponentParameter(2, "ChildContent",
                                (Microsoft.AspNetCore.Components.RenderFragment)(item =>
                                {
                                    item.OpenComponent<NavigationMenuTrigger>(0);
                                    item.AddComponentParameter(1, "Value", "products");
                                    item.AddComponentParameter(2, "ChildContent",
                                        (Microsoft.AspNetCore.Components.RenderFragment)(t =>
                                            t.AddContent(0, "Products")));
                                    item.CloseComponent();
                                }));
                            list.CloseComponent();

                            list.OpenComponent<NavigationMenuItem>(3);
                            list.AddComponentParameter(4, "Value", "docs");
                            list.AddComponentParameter(5, "ChildContent",
                                (Microsoft.AspNetCore.Components.RenderFragment)(item =>
                                {
                                    item.OpenComponent<NavigationMenuTrigger>(0);
                                    item.AddComponentParameter(1, "Value", "docs");
                                    item.AddComponentParameter(2, "ChildContent",
                                        (Microsoft.AspNetCore.Components.RenderFragment)(t =>
                                            t.AddContent(0, "Docs")));
                                    item.CloseComponent();
                                }));
                            list.CloseComponent();
                        }));
                    b.CloseComponent();
                }));
            builder.CloseComponent();
        });
    }

    private Bunit.IRenderedComponent<Bunit.Rendering.ContainerFragment> RenderWithLink(bool active = false)
    {
        return Render(builder =>
        {
            builder.OpenComponent<NavigationMenuRoot>(0);
            builder.AddComponentParameter(1, "ChildContent",
                (Microsoft.AspNetCore.Components.RenderFragment)(b =>
                {
                    b.OpenComponent<NavigationMenuList>(0);
                    b.AddComponentParameter(1, "ChildContent",
                        (Microsoft.AspNetCore.Components.RenderFragment)(list =>
                        {
                            list.OpenComponent<NavigationMenuLink>(0);
                            list.AddComponentParameter(1, "Active", active);
                            list.AddComponentParameter(2, "ChildContent",
                                (Microsoft.AspNetCore.Components.RenderFragment)(l =>
                                    l.AddContent(0, "Home")));
                            list.CloseComponent();
                        }));
                    b.CloseComponent();
                }));
            builder.CloseComponent();
        });
    }

    private Bunit.IRenderedComponent<Bunit.Rendering.ContainerFragment> RenderNavigationMenuWithContent(
        Action<string?>? onValueChanged = null)
    {
        return Render(builder =>
        {
            builder.OpenComponent<NavigationMenuRoot>(0);
            if (onValueChanged is not null)
                builder.AddComponentParameter(1, "ValueChanged",
                    Microsoft.AspNetCore.Components.EventCallback.Factory.Create<string?>(this, onValueChanged));
            builder.AddComponentParameter(2, "ChildContent",
                (Microsoft.AspNetCore.Components.RenderFragment)(b =>
                {
                    b.OpenComponent<NavigationMenuList>(0);
                    b.AddComponentParameter(1, "ChildContent",
                        (Microsoft.AspNetCore.Components.RenderFragment)(list =>
                        {
                            list.OpenComponent<NavigationMenuItem>(0);
                            list.AddComponentParameter(1, "Value", "products");
                            list.AddComponentParameter(2, "ChildContent",
                                (Microsoft.AspNetCore.Components.RenderFragment)(item =>
                                {
                                    item.OpenComponent<NavigationMenuTrigger>(0);
                                    item.AddComponentParameter(1, "Value", "products");
                                    item.AddComponentParameter(2, "ChildContent",
                                        (Microsoft.AspNetCore.Components.RenderFragment)(t =>
                                            t.AddContent(0, "Products")));
                                    item.CloseComponent();

                                    item.OpenComponent<NavigationMenuContent>(3);
                                    item.AddComponentParameter(4, "Value", "products");
                                    item.AddComponentParameter(5, "ChildContent",
                                        (Microsoft.AspNetCore.Components.RenderFragment)(c =>
                                        {
                                            c.OpenComponent<NavigationMenuLink>(0);
                                            c.AddComponentParameter(1, "CloseOnClick", true);
                                            c.AddComponentParameter(2, "ChildContent",
                                                (Microsoft.AspNetCore.Components.RenderFragment)(l =>
                                                    l.AddContent(0, "Product A")));
                                            c.CloseComponent();
                                        }));
                                    item.CloseComponent();
                                }));
                            list.CloseComponent();
                        }));
                    b.CloseComponent();

                    // Viewport renders the portaled content fragments
                    b.OpenComponent<NavigationMenuViewport>(1);
                    b.CloseComponent();
                }));
            builder.CloseComponent();
        });
    }
}
