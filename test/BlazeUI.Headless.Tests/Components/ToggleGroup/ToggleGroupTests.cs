using BlazeUI.Headless.Components.Toggle;
using BlazeUI.Headless.Components.ToggleGroup;
using BlazeUI.Headless.Core;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Headless.Tests.Components.ToggleGroup;

public class ToggleGroupTests : BunitContext
{
    public ToggleGroupTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazeUI();
    }
    [Fact]
    public void RendersDefaultDivTag()
    {
        var cut = Render<ToggleGroupRoot>();
        Assert.Equal("DIV", cut.Find("div").TagName);
    }

    [Fact]
    public void HasGroupRole()
    {
        var cut = Render<ToggleGroupRoot>();
        Assert.Equal("group", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void DataOrientationDefaultsToHorizontal()
    {
        var cut = Render<ToggleGroupRoot>();
        Assert.Equal("horizontal", cut.Find("div").GetAttribute("data-orientation"));
    }

    [Fact]
    public void DataOrientationVertical()
    {
        var cut = Render<ToggleGroupRoot>(p => p
            .Add(c => c.Orientation, BlazeUI.Headless.Core.Orientation.Vertical));
        Assert.Equal("vertical", cut.Find("div").GetAttribute("data-orientation"));
    }

    // data-multiple is present when Multiple=true and absent when Multiple=false,
    // matching the ToggleGroupDataAttributes.multiple mapping in Base UI.
    [Fact]
    public void DataMultiplePresentWhenMultipleIsTrue()
    {
        var cut = Render<ToggleGroupRoot>(p => p.Add(c => c.Multiple, true));
        Assert.NotNull(cut.Find("[data-multiple]"));
    }

    [Fact]
    public void DataMultipleAbsentWhenMultipleIsFalse()
    {
        var cut = Render<ToggleGroupRoot>();
        Assert.Empty(cut.FindAll("[data-multiple]"));
    }

    [Fact]
    public void DataDisabledPresentWhenGroupDisabled()
    {
        var cut = Render<ToggleGroupRoot>(p => p.Add(c => c.Disabled, true));
        Assert.NotNull(cut.Find("[data-disabled]"));
    }

    [Fact]
    public void DataDisabledAbsentWhenGroupEnabled()
    {
        var cut = Render<ToggleGroupRoot>();
        Assert.Empty(cut.FindAll("[data-disabled]"));
    }

    // Single mode: pressing one button deselects the others.
    [Fact]
    public void SingleModeSelectsOneAtATime()
    {
        IReadOnlyList<string>? received = null;
        var cut = Render<ToggleGroupRoot>(p => p
            .Add(c => c.ValueChanged, (IReadOnlyList<string> v) => received = v)
            .AddChildContent<ToggleRoot>(tp => tp.Add(t => t.Value, "a"))
        );

        cut.Find("button").Click();
        Assert.NotNull(received);
        Assert.Single(received!);
        Assert.Equal("a", received![0]);
    }

    [Fact]
    public void SingleModeDeselectsWhenAlreadySelected()
    {
        IReadOnlyList<string>? received = null;
        var cut = Render<ToggleGroupRoot>(p => p
            .Add(c => c.DefaultValue, new[] { "a" })
            .Add(c => c.ValueChanged, (IReadOnlyList<string> v) => received = v)
            .AddChildContent<ToggleRoot>(tp => tp.Add(t => t.Value, "a"))
        );

        // Clicking the already-pressed button should deselect it.
        cut.Find("button").Click();
        Assert.NotNull(received);
        Assert.Empty(received!);
    }

    // Multiple mode: multiple buttons can be pressed simultaneously.
    [Fact]
    public void MultipleModeAllowsMultiplePressed()
    {
        var cut = Render<ToggleGroupRoot>(p => p
            .Add(c => c.Multiple, true)
            .AddChildContent(cb =>
            {
                cb.OpenComponent<ToggleRoot>(0);
                cb.AddComponentParameter(1, "Value", "a");
                cb.CloseComponent();
                cb.OpenComponent<ToggleRoot>(2);
                cb.AddComponentParameter(3, "Value", "b");
                cb.CloseComponent();
            }));

        // Re-query after each click because bUnit re-renders the component.
        cut.FindAll("button")[0].Click();
        cut.FindAll("button")[1].Click();

        Assert.Equal("true", cut.FindAll("button")[0].GetAttribute("aria-pressed"));
        Assert.Equal("true", cut.FindAll("button")[1].GetAttribute("aria-pressed"));
    }

    [Fact]
    public void DefaultValueSetsInitialPressedState()
    {
        var cut = Render<ToggleGroupRoot>(p => p
            .Add(c => c.DefaultValue, new[] { "b" })
            .AddChildContent(cb =>
            {
                cb.OpenComponent<ToggleRoot>(0);
                cb.AddComponentParameter(1, "Value", "a");
                cb.CloseComponent();
                cb.OpenComponent<ToggleRoot>(2);
                cb.AddComponentParameter(3, "Value", "b");
                cb.CloseComponent();
            }));

        var buttons = cut.FindAll("button");
        Assert.Equal("false", buttons[0].GetAttribute("aria-pressed"));
        Assert.Equal("true", buttons[1].GetAttribute("aria-pressed"));
        Assert.NotNull(buttons[1].QuerySelector("[data-pressed]") ?? (buttons[1].HasAttribute("data-pressed") ? buttons[1] : null));
    }

    // Group-level disabled propagates to all child buttons — they gain the HTML
    // disabled attribute and data-disabled, matching Base UI's behaviour.
    [Fact]
    public void GroupDisabledPropagatesDisabledAttributeToItems()
    {
        var cut = Render<ToggleGroupRoot>(p => p
            .Add(c => c.Disabled, true)
            .AddChildContent(cb =>
            {
                cb.OpenComponent<ToggleRoot>(0);
                cb.AddComponentParameter(1, "Value", "a");
                cb.CloseComponent();
                cb.OpenComponent<ToggleRoot>(2);
                cb.AddComponentParameter(3, "Value", "b");
                cb.CloseComponent();
            }));

        var buttons = cut.FindAll("button");
        Assert.True(buttons[0].HasAttribute("disabled"));
        Assert.True(buttons[1].HasAttribute("disabled"));
    }

    [Fact]
    public void GroupDisabledPropagatesDataDisabledToItems()
    {
        var cut = Render<ToggleGroupRoot>(p => p
            .Add(c => c.Disabled, true)
            .AddChildContent(cb =>
            {
                cb.OpenComponent<ToggleRoot>(0);
                cb.AddComponentParameter(1, "Value", "a");
                cb.CloseComponent();
            }));

        Assert.NotNull(cut.Find("button[data-disabled]"));
    }

    [Fact]
    public void GroupDisabledPreventsToggle()
    {
        var cut = Render<ToggleGroupRoot>(p => p
            .Add(c => c.Disabled, true)
            .AddChildContent<ToggleRoot>(tp => tp.Add(t => t.Value, "a"))
        );

        cut.Find("button").Click();
        Assert.Equal("false", cut.Find("button").GetAttribute("aria-pressed"));
    }

    // An individually-disabled Toggle inside an enabled group should still be disabled.
    [Fact]
    public void IndividualItemDisabledIndependently()
    {
        var cut = Render<ToggleGroupRoot>(p => p
            .AddChildContent(cb =>
            {
                cb.OpenComponent<ToggleRoot>(0);
                cb.AddComponentParameter(1, "Value", "a");
                cb.CloseComponent();
                cb.OpenComponent<ToggleRoot>(2);
                cb.AddComponentParameter(3, "Value", "b");
                cb.AddComponentParameter(4, "Disabled", true);
                cb.CloseComponent();
            }));

        var buttons = cut.FindAll("button");
        Assert.False(buttons[0].HasAttribute("disabled"));
        Assert.True(buttons[1].HasAttribute("disabled"));
    }

    // ValueChanged fires with the updated list when an item is toggled.
    [Fact]
    public void ValueChangedFiresOnItemClick()
    {
        IReadOnlyList<string>? received = null;
        var cut = Render<ToggleGroupRoot>(p => p
            .Add(c => c.ValueChanged, (IReadOnlyList<string> v) => received = v)
            .AddChildContent(cb =>
            {
                cb.OpenComponent<ToggleRoot>(0);
                cb.AddComponentParameter(1, "Value", "one");
                cb.CloseComponent();
                cb.OpenComponent<ToggleRoot>(2);
                cb.AddComponentParameter(3, "Value", "two");
                cb.CloseComponent();
            }));

        cut.FindAll("button")[1].Click();
        Assert.NotNull(received);
        Assert.Equal(new[] { "two" }, received);
    }
}
