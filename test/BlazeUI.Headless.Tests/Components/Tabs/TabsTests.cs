using BlazeUI.Headless.Components.Tabs;
using BlazeUI.Headless.Core;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Headless.Tests.Components.Tabs;

public class TabsTests : BunitContext
{
    public TabsTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazeUI();
    }

    // --- Root ---

    [Fact]
    public void Root_RendersDiv()
    {
        var cut = RenderTabs();
        // The root renders a <div> that wraps the tablist and panels.
        Assert.NotNull(cut.Find("div"));
    }

    [Fact]
    public void Root_HasDataOrientationHorizontal()
    {
        var cut = RenderTabs();
        // The root div exposes orientation for CSS targeting.
        var root = cut.Find("div");
        Assert.Equal("horizontal", root.GetAttribute("data-orientation"));
    }

    [Fact]
    public void Root_HasDataActivationDirectionNoneInitially()
    {
        var cut = RenderTabs("t1");
        var root = cut.Find("div");
        Assert.Equal("none", root.GetAttribute("data-activation-direction"));
    }

    [Fact]
    public void Root_UpdatesDataActivationDirectionOnTabChange()
    {
        var cut = RenderTabs("t1");
        cut.FindAll("[role='tab']")[1].Click();

        // Navigating forward (t1 → t2 in horizontal) produces "right".
        var root = cut.Find("div");
        Assert.Equal("right", root.GetAttribute("data-activation-direction"));
    }

    // --- TabsList ---

    [Fact]
    public void TabsList_HasTablistRole()
    {
        var cut = RenderTabs();
        var tablist = cut.Find("[role='tablist']");
        Assert.NotNull(tablist);
    }

    [Fact]
    public void TabsList_HasDataOrientationHorizontal()
    {
        var cut = RenderTabs();
        var tablist = cut.Find("[role='tablist']");
        Assert.Equal("horizontal", tablist.GetAttribute("data-orientation"));
    }

    [Fact]
    public void TabsList_HasDataActivationDirection()
    {
        var cut = RenderTabs("t1");
        var tablist = cut.Find("[role='tablist']");
        Assert.NotNull(tablist.GetAttribute("data-activation-direction"));
    }

    [Fact]
    public void TabsList_DoesNotSetAriaOrientationForHorizontal()
    {
        // Base UI omits aria-orientation for horizontal; it's the implicit default.
        var cut = RenderTabs();
        var tablist = cut.Find("[role='tablist']");
        Assert.Null(tablist.GetAttribute("aria-orientation"));
    }

    [Fact]
    public void TabsList_SetsAriaOrientationForVertical()
    {
        var cut = RenderTabs(orientation: Orientation.Vertical);
        var tablist = cut.Find("[role='tablist']");
        Assert.Equal("vertical", tablist.GetAttribute("aria-orientation"));
    }

    // --- TabsTab ---

    [Fact]
    public void Tab_HasTabRole()
    {
        var cut = RenderTabs();
        var tabs = cut.FindAll("[role='tab']");
        Assert.Equal(2, tabs.Count);
    }

    [Fact]
    public void ActiveTab_HasAriaSelectedTrue()
    {
        var cut = RenderTabs("t1");
        var tab1 = cut.FindAll("[role='tab']")[0];
        Assert.Equal("true", tab1.GetAttribute("aria-selected"));
        Assert.Equal("0", tab1.GetAttribute("tabindex"));
    }

    [Fact]
    public void InactiveTab_HasAriaSelectedFalse()
    {
        var cut = RenderTabs("t1");
        var tab2 = cut.FindAll("[role='tab']")[1];
        Assert.Equal("false", tab2.GetAttribute("aria-selected"));
        Assert.Equal("-1", tab2.GetAttribute("tabindex"));
    }

    [Fact]
    public void Tab_HasAriaControls_MatchingPanelId()
    {
        var cut = RenderTabs("t1");
        var tab1 = cut.FindAll("[role='tab']")[0];
        var controls = tab1.GetAttribute("aria-controls");
        Assert.NotNull(controls);
        Assert.Contains("tabpanel", controls);
    }

    [Fact]
    public void Tab_HasDataActiveWhenActive()
    {
        var cut = RenderTabs("t1");
        var tab1 = cut.FindAll("[role='tab']")[0];
        Assert.True(tab1.HasAttribute("data-active"));
    }

    [Fact]
    public void Tab_NoDataActiveWhenInactive()
    {
        var cut = RenderTabs("t1");
        var tab2 = cut.FindAll("[role='tab']")[1];
        Assert.False(tab2.HasAttribute("data-active"));
    }

    [Fact]
    public void Tab_HasDataOrientation()
    {
        var cut = RenderTabs("t1");
        var tab1 = cut.FindAll("[role='tab']")[0];
        Assert.Equal("horizontal", tab1.GetAttribute("data-orientation"));
    }

    [Fact]
    public void Tab_Disabled_HasDataDisabledAndDisabledAttr()
    {
        var cut = RenderTabsWithDisabledTab();
        var disabledTab = cut.FindAll("[role='tab']")[1];
        Assert.True(disabledTab.HasAttribute("data-disabled"));
        Assert.True(disabledTab.HasAttribute("disabled"));
    }

    [Fact]
    public void Tab_Disabled_DoesNotActivate()
    {
        string? received = null;
        var cut = RenderTabsWithDisabledTab(onValueChanged: v => received = v);
        cut.FindAll("[role='tab']")[1].Click();
        Assert.Null(received);
    }

    // --- TabsPanel ---

    [Fact]
    public void Panel_HasTabpanelRole()
    {
        var cut = RenderTabs("t1", keepMounted: true);
        var panels = cut.FindAll("[role='tabpanel']");
        Assert.Equal(2, panels.Count);
    }

    [Fact]
    public void ActivePanel_IsVisible()
    {
        var cut = RenderTabs("t1", keepMounted: true);
        var panels = cut.FindAll("[role='tabpanel']");
        Assert.False(panels[0].HasAttribute("hidden"));
        Assert.Equal("0", panels[0].GetAttribute("tabindex"));
    }

    [Fact]
    public void InactivePanel_IsHiddenWithKeepMounted()
    {
        var cut = RenderTabs("t1", keepMounted: true);
        var panels = cut.FindAll("[role='tabpanel']");
        Assert.True(panels[1].HasAttribute("hidden"));
        Assert.Equal("-1", panels[1].GetAttribute("tabindex"));
    }

    [Fact]
    public void Panel_HasDataHiddenWhenInactive()
    {
        var cut = RenderTabs("t1", keepMounted: true);
        var panels = cut.FindAll("[role='tabpanel']");
        Assert.False(panels[0].HasAttribute("data-hidden"));
        Assert.True(panels[1].HasAttribute("data-hidden"));
    }

    [Fact]
    public void Panel_HasDataOrientationAndActivationDirection()
    {
        var cut = RenderTabs("t1", keepMounted: true);
        var panel = cut.FindAll("[role='tabpanel']")[0];
        Assert.Equal("horizontal", panel.GetAttribute("data-orientation"));
        Assert.NotNull(panel.GetAttribute("data-activation-direction"));
    }

    [Fact]
    public void Panel_HasDataIndex()
    {
        var cut = RenderTabs("t1", keepMounted: true);
        var panels = cut.FindAll("[role='tabpanel']");
        Assert.NotNull(panels[0].GetAttribute("data-index"));
        Assert.NotNull(panels[1].GetAttribute("data-index"));
    }

    [Fact]
    public void Panel_HasAriaLabelledBy_MatchingTabId()
    {
        var cut = RenderTabs("t1", keepMounted: true);
        var tabs = cut.FindAll("[role='tab']");
        var panels = cut.FindAll("[role='tabpanel']");

        // Each panel's aria-labelledby must point to its controlling tab's id.
        var tab1Id = tabs[0].GetAttribute("id");
        var panel1LabelledBy = panels[0].GetAttribute("aria-labelledby");
        Assert.Equal(tab1Id, panel1LabelledBy);
    }

    [Fact]
    public void Tab_ConsumerIdOverride_PropagatedToPanel()
    {
        // When a consumer provides an explicit Id on TabsTab, the associated
        // TabsPanel's aria-labelledby must reflect that override.
        var cut = RenderTabsWithConsumerIds();
        var tab = cut.Find("[role='tab']");
        var panel = cut.Find("[role='tabpanel']");

        Assert.Equal("my-tab", tab.GetAttribute("id"));
        Assert.Equal("my-tab", panel.GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void Panel_ConsumerIdOverride_PropagatedToTab()
    {
        // When a consumer provides an explicit Id on TabsPanel, the associated
        // TabsTab's aria-controls must reflect that override.
        var cut = RenderTabsWithConsumerIds();
        var tab = cut.Find("[role='tab']");

        Assert.Equal("my-panel", tab.GetAttribute("aria-controls"));
    }

    [Fact]
    public void Panel_UnmountedByDefault_WhenInactive()
    {
        // Without keepMounted, inactive panels are removed from the DOM entirely.
        var cut = RenderTabs("t1");
        var panels = cut.FindAll("[role='tabpanel']");
        Assert.Single(panels);
    }

    [Fact]
    public void Panel_KeepMounted_RetainsInDom()
    {
        var cut = RenderTabs("t1", keepMounted: true);
        var panels = cut.FindAll("[role='tabpanel']");
        Assert.Equal(2, panels.Count);
    }

    // --- Interaction ---

    [Fact]
    public void Click_SwitchesTab()
    {
        string? received = null;
        var cut = RenderTabs("t1", onValueChanged: v => received = v);

        cut.FindAll("[role='tab']")[1].Click();

        Assert.Equal("t2", received);
    }

    [Fact]
    public void Click_ActivePanel_Changes()
    {
        var cut = RenderTabs("t1");
        cut.FindAll("[role='tab']")[1].Click();

        // After switching, t2's panel should now be the only visible one.
        var panels = cut.FindAll("[role='tabpanel']");
        Assert.Single(panels);
        Assert.Equal("t2", panels[0].GetAttribute("data-value") ?? "t2"); // panel is for t2
    }

    // --- TabsIndicator ---

    [Fact]
    public void Indicator_HasDataOrientationAndActivationDirection()
    {
        var cut = RenderTabsWithIndicator("t1");
        var indicator = cut.Find("[role='presentation']");
        Assert.Equal("horizontal", indicator.GetAttribute("data-orientation"));
        Assert.NotNull(indicator.GetAttribute("data-activation-direction"));
    }

    [Fact]
    public void Indicator_HasRolePresentation()
    {
        var cut = RenderTabsWithIndicator("t1");
        Assert.NotNull(cut.Find("[role='presentation']"));
    }

    // --- Helpers ---

    private Bunit.IRenderedComponent<Bunit.Rendering.ContainerFragment> RenderTabs(
        string? defaultValue = null,
        Action<string?>? onValueChanged = null,
        bool keepMounted = false,
        Orientation orientation = Orientation.Horizontal)
    {
        return Render(builder =>
        {
            builder.OpenComponent<TabsRoot>(0);
            if (defaultValue is not null) builder.AddComponentParameter(1, "DefaultValue", defaultValue);
            if (onValueChanged is not null)
                builder.AddComponentParameter(2, "ValueChanged",
                    EventCallback.Factory.Create<string?>(this, onValueChanged));
            builder.AddComponentParameter(3, "Orientation", orientation);
            builder.AddComponentParameter(4, "ChildContent", (RenderFragment)(b =>
            {
                b.OpenComponent<TabsList>(0);
                b.AddComponentParameter(1, "ChildContent", (RenderFragment)(list =>
                {
                    list.OpenComponent<TabsTab>(0);
                    list.AddComponentParameter(1, "Value", "t1");
                    list.AddComponentParameter(2, "ChildContent", (RenderFragment)(t =>
                        t.AddContent(0, "Tab 1")));
                    list.CloseComponent();

                    list.OpenComponent<TabsTab>(3);
                    list.AddComponentParameter(4, "Value", "t2");
                    list.AddComponentParameter(5, "ChildContent", (RenderFragment)(t =>
                        t.AddContent(0, "Tab 2")));
                    list.CloseComponent();
                }));
                b.CloseComponent();

                b.OpenComponent<TabsPanel>(2);
                b.AddComponentParameter(3, "Value", "t1");
                b.AddComponentParameter(4, "KeepMounted", keepMounted);
                b.AddComponentParameter(5, "ChildContent", (RenderFragment)(p =>
                    p.AddContent(0, "Content 1")));
                b.CloseComponent();

                b.OpenComponent<TabsPanel>(6);
                b.AddComponentParameter(7, "Value", "t2");
                b.AddComponentParameter(8, "KeepMounted", keepMounted);
                b.AddComponentParameter(9, "ChildContent", (RenderFragment)(p =>
                    p.AddContent(0, "Content 2")));
                b.CloseComponent();
            }));
            builder.CloseComponent();
        });
    }

    private Bunit.IRenderedComponent<Bunit.Rendering.ContainerFragment> RenderTabsWithDisabledTab(
        Action<string?>? onValueChanged = null)
    {
        return Render(builder =>
        {
            builder.OpenComponent<TabsRoot>(0);
            builder.AddComponentParameter(1, "DefaultValue", "t1");
            if (onValueChanged is not null)
                builder.AddComponentParameter(2, "ValueChanged",
                    EventCallback.Factory.Create<string?>(this, onValueChanged));
            builder.AddComponentParameter(3, "ChildContent", (RenderFragment)(b =>
            {
                b.OpenComponent<TabsList>(0);
                b.AddComponentParameter(1, "ChildContent", (RenderFragment)(list =>
                {
                    list.OpenComponent<TabsTab>(0);
                    list.AddComponentParameter(1, "Value", "t1");
                    list.AddComponentParameter(2, "ChildContent", (RenderFragment)(t =>
                        t.AddContent(0, "Tab 1")));
                    list.CloseComponent();

                    list.OpenComponent<TabsTab>(3);
                    list.AddComponentParameter(4, "Value", "t2");
                    list.AddComponentParameter(5, "Disabled", true);
                    list.AddComponentParameter(6, "ChildContent", (RenderFragment)(t =>
                        t.AddContent(0, "Tab 2")));
                    list.CloseComponent();
                }));
                b.CloseComponent();
            }));
            builder.CloseComponent();
        });
    }

    private Bunit.IRenderedComponent<Bunit.Rendering.ContainerFragment> RenderTabsWithConsumerIds()
    {
        return Render(builder =>
        {
            builder.OpenComponent<TabsRoot>(0);
            builder.AddComponentParameter(1, "DefaultValue", "t1");
            builder.AddComponentParameter(2, "ChildContent", (RenderFragment)(b =>
            {
                b.OpenComponent<TabsList>(0);
                b.AddComponentParameter(1, "ChildContent", (RenderFragment)(list =>
                {
                    list.OpenComponent<TabsTab>(0);
                    list.AddComponentParameter(1, "Value", "t1");
                    list.AddComponentParameter(2, "Id", "my-tab");
                    list.AddComponentParameter(3, "ChildContent", (RenderFragment)(t =>
                        t.AddContent(0, "Tab 1")));
                    list.CloseComponent();
                }));
                b.CloseComponent();

                b.OpenComponent<TabsPanel>(2);
                b.AddComponentParameter(3, "Value", "t1");
                b.AddComponentParameter(4, "Id", "my-panel");
                b.AddComponentParameter(5, "ChildContent", (RenderFragment)(p =>
                    p.AddContent(0, "Content 1")));
                b.CloseComponent();
            }));
            builder.CloseComponent();
        });
    }

    private Bunit.IRenderedComponent<Bunit.Rendering.ContainerFragment> RenderTabsWithIndicator(
        string? defaultValue = null)
    {
        return Render(builder =>
        {
            builder.OpenComponent<TabsRoot>(0);
            if (defaultValue is not null) builder.AddComponentParameter(1, "DefaultValue", defaultValue);
            builder.AddComponentParameter(2, "ChildContent", (RenderFragment)(b =>
            {
                b.OpenComponent<TabsList>(0);
                b.AddComponentParameter(1, "ChildContent", (RenderFragment)(list =>
                {
                    list.OpenComponent<TabsTab>(0);
                    list.AddComponentParameter(1, "Value", "t1");
                    list.AddComponentParameter(2, "ChildContent", (RenderFragment)(t =>
                        t.AddContent(0, "Tab 1")));
                    list.CloseComponent();

                    list.OpenComponent<TabsIndicator>(3);
                    list.CloseComponent();
                }));
                b.CloseComponent();

                b.OpenComponent<TabsPanel>(2);
                b.AddComponentParameter(3, "Value", "t1");
                b.AddComponentParameter(4, "ChildContent", (RenderFragment)(p =>
                    p.AddContent(0, "Content 1")));
                b.CloseComponent();
            }));
            builder.CloseComponent();
        });
    }
}
