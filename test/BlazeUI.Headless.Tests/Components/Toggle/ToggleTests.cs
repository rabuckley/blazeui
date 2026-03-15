using BlazeUI.Headless.Components.Toggle;
using Bunit;

namespace BlazeUI.Headless.Tests.Components.Toggle;

public class ToggleTests : BunitContext
{
    [Fact]
    public void RendersDefaultButtonTag()
    {
        var cut = Render<ToggleRoot>();
        Assert.Equal("BUTTON", cut.Find("button").TagName);
    }

    [Fact]
    public void AriaPressedFalseByDefault()
    {
        var cut = Render<ToggleRoot>();
        Assert.Equal("false", cut.Find("button").GetAttribute("aria-pressed"));
    }

    [Fact]
    public void ClickTogglesPressed()
    {
        var cut = Render<ToggleRoot>();
        cut.Find("button").Click();
        Assert.Equal("true", cut.Find("button").GetAttribute("aria-pressed"));
        Assert.NotNull(cut.Find("[data-pressed]"));
    }

    [Fact]
    public void DataPressedAbsentWhenNotPressed()
    {
        var cut = Render<ToggleRoot>();
        Assert.Empty(cut.FindAll("[data-pressed]"));
    }

    [Fact]
    public void DisabledPreventsToggle()
    {
        var cut = Render<ToggleRoot>(p => p.Add(c => c.Disabled, true));
        cut.Find("button").Click();
        Assert.Equal("false", cut.Find("button").GetAttribute("aria-pressed"));
    }

    [Fact]
    public void ControlledModeReflectsExternalValue()
    {
        var cut = Render<ToggleRoot>(p => p.Add(c => c.Pressed, true));
        Assert.Equal("true", cut.Find("button").GetAttribute("aria-pressed"));
    }

    [Fact]
    public void PressedChangedFiresOnClick()
    {
        bool? received = null;
        var cut = Render<ToggleRoot>(p => p
            .Add(c => c.PressedChanged, (bool v) => received = v));

        cut.Find("button").Click();
        Assert.True(received);
    }

    [Fact]
    public void DefaultPressedSetsInitialState()
    {
        var cut = Render<ToggleRoot>(p => p.Add(c => c.DefaultPressed, true));
        Assert.Equal("true", cut.Find("button").GetAttribute("aria-pressed"));
        Assert.NotNull(cut.Find("[data-pressed]"));
    }
}
