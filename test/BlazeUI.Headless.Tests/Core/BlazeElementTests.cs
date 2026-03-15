using BlazeUI.Headless.Core;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Tests.Core;

/// <summary>
/// Concrete test subclass of <see cref="BlazeElement{TState}"/> for testing the base class.
/// </summary>
file sealed class TestElement : BlazeElement<TestElementState>
{
    [Parameter]
    public bool IsActive { get; set; }

    protected override string DefaultTag => "div";

    protected override TestElementState GetCurrentState() => new(IsActive);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-active", IsActive ? "" : null);
    }
}

file readonly record struct TestElementState(bool IsActive);

public class BlazeElementTests : BunitContext
{
    [Fact]
    public void RendersDefaultTag()
    {
        var cut = Render<TestElement>();
        Assert.Equal("DIV", cut.Find("div").TagName);
    }

    [Fact]
    public void AsParameter_OverridesDefaultTag()
    {
        var cut = Render<TestElement>(p => p.Add(c => c.As, "section"));
        cut.Find("section");
    }

    [Fact]
    public void AutoGeneratesId_WhenIdNotProvided()
    {
        var cut = Render<TestElement>();
        var id = cut.Find("[id]").GetAttribute("id");
        Assert.StartsWith("blazeui-div-", id);
    }

    [Fact]
    public void UsesExplicitId_WhenProvided()
    {
        var cut = Render<TestElement>(p => p.Add(c => c.Id, "my-element"));
        var id = cut.Find("[id]").GetAttribute("id");
        Assert.Equal("my-element", id);
    }

    [Fact]
    public void MergesClassFromParameterAndBuilder()
    {
        var cut = Render<TestElement>(p => p
            .Add(c => c.Class, "static-class")
            .Add(c => c.ClassBuilder, state => state.IsActive ? "active" : "inactive"));

        var cssClass = cut.Find("div").GetAttribute("class");
        Assert.Equal("static-class inactive", cssClass);
    }

    [Fact]
    public void DataAttributes_IncludedWhenValueIsNotNull()
    {
        var cut = Render<TestElement>(p => p.Add(c => c.IsActive, true));
        Assert.NotNull(cut.Find("[data-active]"));
    }

    [Fact]
    public void DataAttributes_OmittedWhenValueIsNull()
    {
        var cut = Render<TestElement>(p => p.Add(c => c.IsActive, false));
        Assert.Empty(cut.FindAll("[data-active]"));
    }

    [Fact]
    public void SplatsAdditionalAttributes()
    {
        var cut = Render<TestElement>(p => p
            .AddUnmatched("aria-label", "test")
            .AddUnmatched("data-testid", "element"));

        var el = cut.Find("div");
        Assert.Equal("test", el.GetAttribute("aria-label"));
        Assert.Equal("element", el.GetAttribute("data-testid"));
    }

    [Fact]
    public void RendersChildContent()
    {
        var cut = Render<TestElement>(p => p
            .AddChildContent("<span>Hello</span>"));

        Assert.Equal("Hello", cut.Find("span").TextContent);
    }
}
