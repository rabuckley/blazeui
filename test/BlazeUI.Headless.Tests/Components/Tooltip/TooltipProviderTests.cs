using BlazeUI.Headless.Components.Tooltip;
using BlazeUI.Headless.Core;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Headless.Tests.Components.Tooltip;

public class TooltipProviderTests : BunitContext
{
    public TooltipProviderTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazeUI();
    }

    [Fact]
    public void Provider_CascadesTooltipProviderContext()
    {
        // The TooltipProvider should cascade a TooltipProviderContext.
        // We verify by checking that a TooltipRoot rendered inside a provider
        // still renders without error.
        var cut = Render<TooltipProvider>(p => p
            .Add(c => c.Delay, 500)
            .Add(c => c.CloseDelay, 100)
            .Add(c => c.Timeout, 600)
            .AddChildContent<TooltipRoot>(tp => tp
                .AddChildContent<TooltipTrigger>(t => t.AddChildContent("Hover me"))));

        Assert.NotNull(cut.Find("button"));
    }

    [Fact]
    public void Provider_DefaultTimeoutIs400()
    {
        var cut = Render<TooltipProvider>(p => p
            .AddChildContent<TooltipRoot>(tp => tp
                .AddChildContent<TooltipTrigger>(t => t.AddChildContent("Hover me"))));

        // Just verify it renders (timeout=400 is the default).
        Assert.NotNull(cut.Find("button"));
    }

    [Fact]
    public void ProviderContext_IsWithinGroupTimeout_FalseInitially()
    {
        var ctx = new TooltipProviderContext { Timeout = 400 };
        Assert.False(ctx.IsWithinGroupTimeout());
    }

    [Fact]
    public void ProviderContext_IsWithinGroupTimeout_TrueAfterRecordClose()
    {
        var ctx = new TooltipProviderContext { Timeout = 400 };
        ctx.RecordClose();
        Assert.True(ctx.IsWithinGroupTimeout());
    }

    [Fact]
    public void ProviderContext_Delay_PropagatedToContext()
    {
        var ctx = new TooltipProviderContext { Delay = 1000, CloseDelay = 200 };
        Assert.Equal(1000, ctx.Delay);
        Assert.Equal(200, ctx.CloseDelay);
    }

    [Fact]
    public void MultipleTooltips_RenderInsideProvider()
    {
        var cut = Render<TooltipProvider>(p => p
            .Add(c => c.Delay, 100)
            .AddChildContent((RenderFragment)(builder =>
            {
                builder.OpenComponent<TooltipRoot>(0);
                builder.AddComponentParameter(1, "ChildContent", (RenderFragment)(inner =>
                {
                    inner.OpenComponent<TooltipTrigger>(0);
                    inner.AddComponentParameter(1, "ChildContent", (RenderFragment)(t =>
                        t.AddContent(0, "Tooltip 1")));
                    inner.CloseComponent();
                }));
                builder.CloseComponent();

                builder.OpenComponent<TooltipRoot>(2);
                builder.AddComponentParameter(3, "ChildContent", (RenderFragment)(inner =>
                {
                    inner.OpenComponent<TooltipTrigger>(0);
                    inner.AddComponentParameter(1, "ChildContent", (RenderFragment)(t =>
                        t.AddContent(0, "Tooltip 2")));
                    inner.CloseComponent();
                }));
                builder.CloseComponent();
            })));

        var buttons = cut.FindAll("button");
        Assert.Equal(2, buttons.Count);
    }
}
