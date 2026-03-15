using BlazeUI.Headless.Components.Collapsible;
using BlazeUI.Headless.Core;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Headless.Tests.Components.Collapsible;

public class CollapsibleTests : BunitContext
{
    public CollapsibleTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazeUI();
    }

    // ---------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------

    private Bunit.IRenderedComponent<Bunit.Rendering.ContainerFragment> RenderCollapsible(
        bool defaultOpen = false,
        bool? open = null,
        bool disabled = false,
        bool keepMounted = false,
        EventCallback<bool>? openChanged = null)
    {
        return Render(builder =>
        {
            builder.OpenComponent<CollapsibleRoot>(0);
            if (open.HasValue)
                builder.AddComponentParameter(1, "Open", open.Value);
            else
                builder.AddComponentParameter(1, "DefaultOpen", defaultOpen);
            if (disabled) builder.AddComponentParameter(2, "Disabled", true);
            if (openChanged.HasValue) builder.AddComponentParameter(3, "OpenChanged", openChanged.Value);
            builder.AddComponentParameter(4, "ChildContent", (RenderFragment)(b =>
            {
                b.OpenComponent<CollapsibleTrigger>(0);
                b.AddComponentParameter(1, "ChildContent", (RenderFragment)(inner =>
                    inner.AddContent(0, "Toggle")));
                b.CloseComponent();
                b.OpenComponent<CollapsiblePanel>(2);
                b.AddComponentParameter(3, "KeepMounted", keepMounted);
                b.AddComponentParameter(4, "ChildContent", (RenderFragment)(inner =>
                    inner.AddContent(0, "Content")));
                b.CloseComponent();
            }));
            builder.CloseComponent();
        });
    }

    // ---------------------------------------------------------------------------
    // Trigger ARIA attributes
    // ---------------------------------------------------------------------------

    [Fact]
    public void Trigger_RendersButtonByDefault()
    {
        var cut = RenderCollapsible();
        var trigger = cut.Find("button");
        Assert.Equal("BUTTON", trigger.TagName);
    }

    [Fact]
    public void Trigger_HasAriaExpandedFalse_WhenClosed()
    {
        var cut = RenderCollapsible();
        Assert.Equal("false", cut.Find("button").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void Trigger_HasAriaExpandedTrue_WhenOpen()
    {
        var cut = RenderCollapsible(defaultOpen: true);
        Assert.Equal("true", cut.Find("button").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void Trigger_HasNoAriaControls_WhenClosed()
    {
        // When the panel is closed (and unmounted by default), aria-controls is
        // omitted so the trigger does not reference a non-existent element.
        var cut = RenderCollapsible();
        Assert.Null(cut.Find("button").GetAttribute("aria-controls"));
    }

    [Fact]
    public void Trigger_HasAriaControls_WhenOpen()
    {
        var cut = RenderCollapsible(defaultOpen: true);
        var trigger = cut.Find("button");
        // Panel renders as a plain div with an id (no role="region" in Base UI).
        var panel = cut.Find("div[id]");
        Assert.Equal(panel.GetAttribute("id"), trigger.GetAttribute("aria-controls"));
    }

    // ---------------------------------------------------------------------------
    // Panel rendering and ARIA
    // ---------------------------------------------------------------------------

    [Fact]
    public void Panel_IsUnmounted_WhenClosedByDefault()
    {
        // Default behaviour: panel is removed from the DOM when closed.
        // Base UI's CollapsiblePanel has no role="region" or aria-labelledby.
        var cut = RenderCollapsible();
        Assert.Empty(cut.FindAll("div[id]"));
    }

    [Fact]
    public void Panel_IsRendered_WhenOpen()
    {
        // Base UI's CollapsiblePanel renders as a plain div with an id — no role="region".
        var cut = RenderCollapsible(defaultOpen: true);
        var panel = cut.Find("div[id]");
        Assert.NotNull(panel);
        Assert.False(panel.HasAttribute("hidden"));
    }

    [Fact]
    public void Panel_HasDataOpen_WhenOpen()
    {
        var cut = RenderCollapsible(defaultOpen: true);
        Assert.NotNull(cut.Find("[data-open]"));
        Assert.Empty(cut.FindAll("[data-closed]"));
    }

    // ---------------------------------------------------------------------------
    // KeepMounted prop
    // ---------------------------------------------------------------------------

    [Fact]
    public void Panel_KeepMounted_RemainsInDom_WhenClosed()
    {
        // Base UI's CollapsiblePanel has no role="region"; it's a plain div with an id.
        var cut = RenderCollapsible(keepMounted: true);
        var panel = cut.Find("div[id]");
        Assert.NotNull(panel);
        Assert.True(panel.HasAttribute("hidden"));
        Assert.NotNull(panel.GetAttribute("data-closed"));
    }

    [Fact]
    public void Panel_KeepMounted_NotHidden_WhenOpen()
    {
        // Base UI's CollapsiblePanel has no role="region"; it's a plain div with an id.
        var cut = RenderCollapsible(defaultOpen: true, keepMounted: true);
        var panel = cut.Find("div[id]");
        Assert.False(panel.HasAttribute("hidden"));
        Assert.NotNull(panel.GetAttribute("data-open"));
    }

    // ---------------------------------------------------------------------------
    // Disabled state
    // ---------------------------------------------------------------------------

    [Fact]
    public void Disabled_TriggerHasAriaDisabled()
    {
        // Base UI uses focusableWhenDisabled: true — disabled state is communicated via
        // aria-disabled rather than the native disabled attribute so the trigger stays focusable.
        var cut = RenderCollapsible(disabled: true);
        var trigger = cut.Find("button");
        Assert.Equal("true", trigger.GetAttribute("aria-disabled"));
        Assert.False(trigger.HasAttribute("disabled"));
        Assert.NotNull(trigger.GetAttribute("data-disabled"));
    }

    [Fact]
    public void Root_HasDataDisabled_WhenDisabled()
    {
        var cut = RenderCollapsible(disabled: true);
        // The root div carries the data-disabled attribute.
        Assert.NotNull(cut.Find("[data-disabled]"));
    }

    // ---------------------------------------------------------------------------
    // Open/close state transitions
    // ---------------------------------------------------------------------------

    [Fact]
    public void Click_TogglesOpenState()
    {
        var cut = RenderCollapsible();
        var trigger = cut.Find("button");
        trigger.Click();
        Assert.Equal("true", cut.Find("button").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void Click_MountsAndUnmountsPanel()
    {
        // Base UI's CollapsiblePanel has no role="region"; it's a plain div with an id.
        var cut = RenderCollapsible();

        // Closed: panel not in DOM.
        Assert.Empty(cut.FindAll("div[id]"));

        cut.Find("button").Click();

        // Open: panel in DOM.
        Assert.NotNull(cut.Find("div[id]"));
        Assert.NotNull(cut.Find("[data-open]"));

        cut.Find("button").Click();

        // Closed again: panel removed.
        Assert.Empty(cut.FindAll("div[id]"));
    }

    [Fact]
    public void DefaultOpen_PanelNotHidden()
    {
        // Base UI's CollapsiblePanel has no role="region"; it's a plain div with an id.
        var cut = RenderCollapsible(defaultOpen: true);
        var panel = cut.Find("div[id]");
        Assert.False(panel.HasAttribute("hidden"));
        Assert.Equal("true", cut.Find("button").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void OpenChanged_FiresOnToggle()
    {
        bool? received = null;
        var callback = EventCallback.Factory.Create<bool>(this, v => received = v);
        var cut = RenderCollapsible(openChanged: callback);
        cut.Find("button").Click();
        Assert.True(received);
    }

    [Fact]
    public void ControlledOpen_ReflectsExternalState()
    {
        var cut = RenderCollapsible(open: false);

        Assert.Equal("false", cut.Find("button").GetAttribute("aria-expanded"));
        // Panel has no role="region"; closed means it is unmounted entirely.
        Assert.Empty(cut.FindAll("div[id]"));
    }

    // ---------------------------------------------------------------------------
    // Root element state attributes
    // ---------------------------------------------------------------------------

    [Fact]
    public void Root_HasDataClosed_WhenClosed()
    {
        var cut = RenderCollapsible();
        // The root div should carry data-closed when closed.
        Assert.NotNull(cut.Find("[data-closed]"));
    }

    [Fact]
    public void Root_HasDataOpen_WhenOpen()
    {
        var cut = RenderCollapsible(defaultOpen: true);
        // The root div carries data-open; panel also carries it.
        // Verify at least one element has data-open (root + panel).
        Assert.NotEmpty(cut.FindAll("[data-open]"));
    }
}
