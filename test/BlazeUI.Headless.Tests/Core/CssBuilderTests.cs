using BlazeUI.Headless.Core;

namespace BlazeUI.Headless.Tests.Core;

public class CssBuilderTests
{
    [Fact]
    public void Cn_CombinesMultipleClassNames()
    {
        var result = Css.Cn("btn", "btn-primary");
        Assert.Equal("btn btn-primary", result);
    }

    [Fact]
    public void Cn_SkipsNullValues()
    {
        var result = Css.Cn("btn", null, "active");
        Assert.Equal("btn active", result);
    }

    [Fact]
    public void Cn_SkipsEmptyAndWhitespaceStrings()
    {
        var result = Css.Cn("btn", "", "  ", "active");
        Assert.Equal("btn active", result);
    }

    [Fact]
    public void Cn_ReturnsEmptyStringWhenAllNull()
    {
        var result = Css.Cn(null, null);
        Assert.Equal("", result);
    }

    [Fact]
    public void Cn_ReturnsEmptyStringForNoArgs()
    {
        var result = Css.Cn(Array.Empty<string?>());
        Assert.Equal("", result);
    }

    [Fact]
    public void ConditionalCn_IncludesOnlyTrueConditions()
    {
        var result = Css.Cn(
            ("btn", true),
            ("disabled", false),
            ("active", true));

        Assert.Equal("btn active", result);
    }

    [Fact]
    public void ConditionalCn_SkipsNullClassNames()
    {
        var result = Css.Cn(
            (null, true),
            ("btn", true));

        Assert.Equal("btn", result);
    }

    [Fact]
    public void ConditionalCn_ReturnsEmptyWhenAllFalse()
    {
        var result = Css.Cn(
            ("btn", false),
            ("active", false));

        Assert.Equal("", result);
    }
}
