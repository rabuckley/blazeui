using BlazeUI.Headless.Core;
using Bunit;
using SeparatorComponent = BlazeUI.Headless.Components.Separator.Separator;

namespace BlazeUI.Headless.Tests.Components.Separator;

public sealed class SeparatorTests : BunitContext
{
    [Fact]
    public void Renders_default_div_tag()
    {
        var cut = Render<SeparatorComponent>();

        var element = cut.Find("div[role='separator']");

        Assert.NotNull(element);
    }

    [Fact]
    public void As_parameter_overrides_tag()
    {
        var cut = Render<SeparatorComponent>(parameters =>
            parameters.Add(p => p.As, "div"));

        var element = cut.Find("div[role='separator']");

        Assert.NotNull(element);
    }

    [Fact]
    public void Data_orientation_defaults_to_horizontal()
    {
        var cut = Render<SeparatorComponent>();

        var element = cut.Find("[data-orientation='horizontal']");

        Assert.NotNull(element);
    }

    [Fact]
    public void Data_orientation_vertical_when_set()
    {
        var cut = Render<SeparatorComponent>(parameters =>
            parameters.Add(p => p.Orientation, Orientation.Vertical));

        var element = cut.Find("[data-orientation='vertical']");

        Assert.NotNull(element);
    }

    [Fact]
    public void Role_separator_is_present()
    {
        var cut = Render<SeparatorComponent>();

        var element = cut.Find("[role='separator']");

        Assert.NotNull(element);
    }

    [Fact]
    public void Aria_orientation_matches_orientation()
    {
        var cut = Render<SeparatorComponent>(parameters =>
            parameters.Add(p => p.Orientation, Orientation.Vertical));

        var element = cut.Find("[aria-orientation='vertical']");

        Assert.NotNull(element);
    }
}
