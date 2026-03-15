using BlazeUI.Headless.Components.Accordion;
using BlazeUI.Headless.Core;
using Bunit;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Headless.Tests.Components.Accordion;

public class AccordionTests : BunitContext
{
    public AccordionTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazeUI();
    }

    [Fact]
    public void Root_HasRegionRole()
    {
        var cut = RenderAccordion();
        var root = cut.Find("[role='region']");
        Assert.NotNull(root);
    }

    [Fact]
    public void Root_HasOrientationDataAttribute()
    {
        var cut = RenderAccordion();
        // vertical is the default orientation
        Assert.Equal("vertical", cut.Find("[role='region']").GetAttribute("data-orientation"));
    }

    [Fact]
    public void Root_Disabled_HasDataDisabled()
    {
        var cut = Render(builder =>
        {
            builder.OpenComponent<AccordionRoot>(0);
            builder.AddComponentParameter(1, "Disabled", true);
            builder.CloseComponent();
        });

        Assert.NotNull(cut.Find("[data-disabled]"));
    }

    [Fact]
    public void Trigger_HasAriaExpandedFalse()
    {
        var cut = RenderAccordion();
        var trigger = cut.Find("[data-testid='trigger-1']");
        Assert.Equal("false", trigger.GetAttribute("aria-expanded"));
    }

    [Fact]
    public void Trigger_HasAriaControls_ReferencingPanel_WhenOpen()
    {
        // aria-controls is only emitted when the item is open, matching Base UI's behaviour
        // where the attribute is set to undefined (omitted) when closed.
        var cut = RenderAccordion();
        cut.Find("[data-testid='trigger-1']").Click();

        var trigger = cut.Find("[data-testid='trigger-1']");
        var panel = cut.Find("[role='region'][aria-labelledby]");
        Assert.Equal(panel.Id, trigger.GetAttribute("aria-controls"));
    }

    [Fact]
    public void Panel_HasRegionRole()
    {
        var cut = RenderAccordion();
        var panel = cut.Find("[role='region'][aria-labelledby]");
        Assert.NotNull(panel);
        Assert.True(panel.HasAttribute("hidden"));
    }

    [Fact]
    public void Panel_AriaLabelledBy_ReferencesTrigger()
    {
        var cut = RenderAccordion();
        var trigger = cut.Find("[data-testid='trigger-1']");
        var panel = cut.Find("[role='region'][aria-labelledby]");
        Assert.Equal(trigger.Id, panel.GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void Click_OpensItem()
    {
        var cut = RenderAccordion();
        cut.Find("[data-testid='trigger-1']").Click();
        Assert.Equal("true", cut.Find("[data-testid='trigger-1']").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void Click_OpensItem_RemovesHiddenFromPanel()
    {
        var cut = RenderAccordion();
        cut.Find("[data-testid='trigger-1']").Click();
        // The panel with aria-labelledby (item panel) should no longer be hidden.
        var panel = cut.Find("[role='region'][aria-labelledby]");
        Assert.False(panel.HasAttribute("hidden"));
    }

    [Fact]
    public void Panel_Open_HasDataOpenAttribute()
    {
        var cut = RenderAccordion();
        cut.Find("[data-testid='trigger-1']").Click();
        var panel = cut.Find("[role='region'][aria-labelledby]");
        Assert.True(panel.HasAttribute("data-open"));
    }

    [Fact]
    public void SingleMode_ClosesOtherItems()
    {
        IReadOnlyList<string>? received = null;
        var cut = RenderAccordion(onValueChanged: v => received = v);

        cut.Find("[data-testid='trigger-1']").Click();
        Assert.NotNull(received);
        Assert.Single(received!);
        Assert.Equal("a", received![0]);
    }

    [Fact]
    public void MultipleMode_AllowsMultipleOpenItems()
    {
        IReadOnlyList<string>? received = null;
        var cut = RenderAccordion(openMultiple: true, onValueChanged: v => received = v);

        cut.Find("[data-testid='trigger-1']").Click();
        cut.Find("[data-testid='trigger-2']").Click();

        Assert.NotNull(received);
        Assert.Equal(2, received!.Count);
    }

    [Fact]
    public void Header_DefaultsToH3()
    {
        var cut = RenderAccordion();
        var header = cut.Find("h3");
        Assert.Equal("H3", header.TagName);
    }

    [Fact]
    public void Header_HasDataIndexAttribute()
    {
        var cut = RenderAccordion();
        var header = cut.Find("h3");
        Assert.Equal("0", header.GetAttribute("data-index"));
    }

    [Fact]
    public void Header_Open_HasDataOpenAttribute()
    {
        var cut = RenderAccordion();
        cut.Find("[data-testid='trigger-1']").Click();
        var header = cut.Find("h3");
        Assert.True(header.HasAttribute("data-open"));
    }

    [Fact]
    public void Panel_HasDataIndexAttribute()
    {
        var cut = RenderAccordion();
        var panel = cut.Find("[role='region'][aria-labelledby]");
        Assert.Equal("0", panel.GetAttribute("data-index"));
    }

    [Fact]
    public void Panel_HasDataOrientationAttribute()
    {
        var cut = RenderAccordion();
        var panel = cut.Find("[role='region'][aria-labelledby]");
        Assert.Equal("vertical", panel.GetAttribute("data-orientation"));
    }

    [Fact]
    public void DisabledItem_HasDataDisabled()
    {
        var cut = Render(builder =>
        {
            builder.OpenComponent<AccordionRoot>(0);
            builder.AddComponentParameter(1, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(b =>
            {
                b.OpenComponent<AccordionItem>(0);
                b.AddComponentParameter(1, "Value", "d");
                b.AddComponentParameter(2, "Disabled", true);
                b.AddComponentParameter(3, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(inner =>
                {
                    inner.OpenComponent<AccordionHeader>(0);
                    inner.AddComponentParameter(1, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(h =>
                    {
                        h.OpenComponent<AccordionTrigger>(0);
                        h.AddComponentParameter(1, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(t =>
                            t.AddContent(0, "Disabled")));
                        h.CloseComponent();
                    }));
                    inner.CloseComponent();
                }));
                b.CloseComponent();
            }));
            builder.CloseComponent();
        });

        Assert.NotNull(cut.Find("[data-disabled]"));
    }

    [Fact]
    public void DisabledItem_TriggerHasDisabledAttribute()
    {
        var cut = Render(builder =>
        {
            builder.OpenComponent<AccordionRoot>(0);
            builder.AddComponentParameter(1, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(b =>
            {
                b.OpenComponent<AccordionItem>(0);
                b.AddComponentParameter(1, "Value", "d");
                b.AddComponentParameter(2, "Disabled", true);
                b.AddComponentParameter(3, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(inner =>
                {
                    inner.OpenComponent<AccordionHeader>(0);
                    inner.AddComponentParameter(1, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(h =>
                    {
                        h.OpenComponent<AccordionTrigger>(0);
                        h.AddComponentParameter(1, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(t =>
                            t.AddContent(0, "Disabled")));
                        h.CloseComponent();
                    }));
                    inner.CloseComponent();
                }));
                b.CloseComponent();
            }));
            builder.CloseComponent();
        });

        var trigger = cut.Find("button");
        Assert.True(trigger.HasAttribute("disabled"));
    }

    [Fact]
    public void DefaultValue_ItemOpenInitially()
    {
        var cut = Render(builder =>
        {
            builder.OpenComponent<AccordionRoot>(0);
            builder.AddComponentParameter(1, "DefaultValue", (IReadOnlyList<string>)["a"]);
            builder.AddComponentParameter(2, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(b =>
            {
                b.OpenComponent<AccordionItem>(0);
                b.AddComponentParameter(1, "Value", "a");
                b.AddComponentParameter(2, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(inner =>
                {
                    inner.OpenComponent<AccordionHeader>(0);
                    inner.AddComponentParameter(1, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(h =>
                    {
                        h.OpenComponent<AccordionTrigger>(0);
                        h.AddComponentParameter(1, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(t =>
                            t.AddContent(0, "Item A")));
                        h.CloseComponent();
                    }));
                    inner.CloseComponent();
                    inner.OpenComponent<AccordionPanel>(2);
                    inner.AddComponentParameter(3, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(p =>
                        p.AddContent(0, "Panel A")));
                    inner.CloseComponent();
                }));
                b.CloseComponent();
            }));
            builder.CloseComponent();
        });

        Assert.Equal("true", cut.Find("button").GetAttribute("aria-expanded"));
        Assert.False(cut.Find("[role='region'][aria-labelledby]").HasAttribute("hidden"));
    }

    [Fact]
    public void Item_HasDataIndexAttribute()
    {
        var cut = RenderAccordion();
        // First item has index 0
        var firstItem = cut.Find("[data-testid='item-1']");
        Assert.Equal("0", firstItem.GetAttribute("data-index"));
        // Second item has index 1
        var secondItem = cut.Find("[data-testid='item-2']");
        Assert.Equal("1", secondItem.GetAttribute("data-index"));
    }

    private Bunit.IRenderedComponent<Bunit.Rendering.ContainerFragment> RenderAccordion(
        bool openMultiple = false,
        Action<IReadOnlyList<string>>? onValueChanged = null)
    {
        return Render(builder =>
        {
            builder.OpenComponent<AccordionRoot>(0);
            builder.AddComponentParameter(1, "OpenMultiple", openMultiple);
            if (onValueChanged is not null)
                builder.AddComponentParameter(2, "ValueChanged", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<IReadOnlyList<string>>(this, onValueChanged));
            builder.AddComponentParameter(3, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(b =>
            {
                b.OpenComponent<AccordionItem>(0);
                b.AddComponentParameter(1, "Value", "a");
                b.AddAttribute(2, "data-testid", "item-1");
                b.AddComponentParameter(3, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(inner =>
                {
                    inner.OpenComponent<AccordionHeader>(0);
                    inner.AddComponentParameter(1, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(h =>
                    {
                        h.OpenComponent<AccordionTrigger>(0);
                        h.AddAttribute(1, "data-testid", "trigger-1");
                        h.AddComponentParameter(2, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(t =>
                            t.AddContent(0, "Item 1")));
                        h.CloseComponent();
                    }));
                    inner.CloseComponent();
                    inner.OpenComponent<AccordionPanel>(2);
                    inner.AddComponentParameter(3, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(p =>
                        p.AddContent(0, "Content 1")));
                    inner.CloseComponent();
                }));
                b.CloseComponent();

                b.OpenComponent<AccordionItem>(4);
                b.AddComponentParameter(5, "Value", "b");
                b.AddAttribute(6, "data-testid", "item-2");
                b.AddComponentParameter(7, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(inner =>
                {
                    inner.OpenComponent<AccordionHeader>(0);
                    inner.AddComponentParameter(1, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(h =>
                    {
                        h.OpenComponent<AccordionTrigger>(0);
                        h.AddAttribute(1, "data-testid", "trigger-2");
                        h.AddComponentParameter(2, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(t =>
                            t.AddContent(0, "Item 2")));
                        h.CloseComponent();
                    }));
                    inner.CloseComponent();
                    inner.OpenComponent<AccordionPanel>(2);
                    inner.AddComponentParameter(3, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(p =>
                        p.AddContent(0, "Content 2")));
                    inner.CloseComponent();
                }));
                b.CloseComponent();
            }));
            builder.CloseComponent();
        });
    }
}
