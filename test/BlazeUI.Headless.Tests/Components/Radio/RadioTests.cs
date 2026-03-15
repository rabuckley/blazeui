using BlazeUI.Headless.Components.Radio;
using BlazeUI.Headless.Core;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Headless.Tests.Components.Radio;

public class RadioTests : BunitContext
{
    public RadioTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazeUI();
    }
    // --- RadioGroupRoot ---

    [Fact]
    public void Group_RendersDefaultDivTag()
    {
        var cut = Render<RadioGroupRoot>();
        Assert.Equal("DIV", cut.Find("div").TagName);
    }

    [Fact]
    public void Group_HasRadiogroupRole()
    {
        var cut = Render<RadioGroupRoot>();
        Assert.Equal("radiogroup", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void Group_Disabled_SetsAriaDisabled()
    {
        var cut = Render<RadioGroupRoot>(p => p.Add(c => c.Disabled, true));
        Assert.Equal("true", cut.Find("[role=radiogroup]").GetAttribute("aria-disabled"));
    }

    [Fact]
    public void Group_Disabled_SetsDataDisabled()
    {
        var cut = Render<RadioGroupRoot>(p => p.Add(c => c.Disabled, true));
        Assert.Equal("", cut.Find("[role=radiogroup]").GetAttribute("data-disabled"));
    }

    [Fact]
    public void Group_ReadOnly_SetsAriaReadonly()
    {
        var cut = Render<RadioGroupRoot>(p => p.Add(c => c.ReadOnly, true));
        Assert.Equal("true", cut.Find("[role=radiogroup]").GetAttribute("aria-readonly"));
    }

    [Fact]
    public void Group_Required_SetsAriaRequired()
    {
        var cut = Render<RadioGroupRoot>(p => p.Add(c => c.Required, true));
        Assert.Equal("true", cut.Find("[role=radiogroup]").GetAttribute("aria-required"));
    }

    [Fact]
    public void Group_NotDisabled_OmitsAriaDisabled()
    {
        var cut = Render<RadioGroupRoot>();
        Assert.Null(cut.Find("[role=radiogroup]").GetAttribute("aria-disabled"));
    }

    // --- RadioRoot ---

    [Fact]
    public void Item_HasRadioRole()
    {
        var cut = Render<RadioGroupRoot>(p => p
            .AddChildContent<RadioRoot>(rp => rp.Add(r => r.Value, "opt1")));
        Assert.Equal("radio", cut.Find("[role=radio]").GetAttribute("role"));
    }

    [Fact]
    public void Item_Unchecked_HasAriaCheckedFalse()
    {
        var cut = Render<RadioGroupRoot>(p => p
            .AddChildContent<RadioRoot>(rp => rp.Add(r => r.Value, "opt1")));
        Assert.Equal("false", cut.Find("[role=radio]").GetAttribute("aria-checked"));
    }

    [Fact]
    public void Item_Checked_HasAriaCheckedTrue()
    {
        var cut = Render<RadioGroupRoot>(p => p
            .Add(c => c.Value, "opt1")
            .AddChildContent<RadioRoot>(rp => rp.Add(r => r.Value, "opt1")));
        Assert.Equal("true", cut.Find("[role=radio]").GetAttribute("aria-checked"));
    }

    [Fact]
    public void Item_Checked_HasDataChecked()
    {
        var cut = Render<RadioGroupRoot>(p => p
            .Add(c => c.Value, "opt1")
            .AddChildContent<RadioRoot>(rp => rp.Add(r => r.Value, "opt1")));
        var radio = cut.Find("[role=radio]");
        Assert.Equal("", radio.GetAttribute("data-checked"));
        Assert.Null(radio.GetAttribute("data-unchecked"));
    }

    [Fact]
    public void Item_Unchecked_HasDataUnchecked()
    {
        var cut = Render<RadioGroupRoot>(p => p
            .AddChildContent<RadioRoot>(rp => rp.Add(r => r.Value, "opt1")));
        var radio = cut.Find("[role=radio]");
        Assert.Null(radio.GetAttribute("data-checked"));
        Assert.Equal("", radio.GetAttribute("data-unchecked"));
    }

    [Fact]
    public void Item_Required_SetsAriaRequired()
    {
        var cut = Render<RadioGroupRoot>(p => p
            .Add(c => c.Required, true)
            .AddChildContent<RadioRoot>(rp => rp.Add(r => r.Value, "opt1")));
        Assert.Equal("true", cut.Find("[role=radio]").GetAttribute("aria-required"));
    }

    [Fact]
    public void Item_Required_SetsDataRequired()
    {
        var cut = Render<RadioGroupRoot>(p => p
            .Add(c => c.Required, true)
            .AddChildContent<RadioRoot>(rp => rp.Add(r => r.Value, "opt1")));
        Assert.Equal("", cut.Find("[role=radio]").GetAttribute("data-required"));
    }

    [Fact]
    public void Item_ReadOnly_SetsAriaReadonly()
    {
        var cut = Render<RadioGroupRoot>(p => p
            .Add(c => c.ReadOnly, true)
            .AddChildContent<RadioRoot>(rp => rp.Add(r => r.Value, "opt1")));
        Assert.Equal("true", cut.Find("[role=radio]").GetAttribute("aria-readonly"));
    }

    [Fact]
    public void Item_ReadOnly_SetsDataReadonly()
    {
        var cut = Render<RadioGroupRoot>(p => p
            .Add(c => c.ReadOnly, true)
            .AddChildContent<RadioRoot>(rp => rp.Add(r => r.Value, "opt1")));
        Assert.Equal("", cut.Find("[role=radio]").GetAttribute("data-readonly"));
    }

    [Fact]
    public void Item_Disabled_SetsAriaDisabled()
    {
        var cut = Render<RadioGroupRoot>(p => p
            .AddChildContent<RadioRoot>(rp => rp
                .Add(r => r.Value, "opt1")
                .Add(r => r.Disabled, true)));
        Assert.Equal("true", cut.Find("[role=radio]").GetAttribute("aria-disabled"));
    }

    [Fact]
    public void Click_SelectsRadio()
    {
        string? received = null;
        var cut = Render<RadioGroupRoot>(p => p
            .Add(c => c.ValueChanged, (string? v) => received = v)
            .AddChildContent<RadioRoot>(rp => rp.Add(r => r.Value, "opt1")));

        cut.Find("[role=radio]").Click();
        Assert.Equal("opt1", received);
    }

    [Fact]
    public void DisabledGroup_PreventsSelection()
    {
        string? received = null;
        var cut = Render<RadioGroupRoot>(p => p
            .Add(c => c.Disabled, true)
            .Add(c => c.ValueChanged, (string? v) => received = v)
            .AddChildContent<RadioRoot>(rp => rp.Add(r => r.Value, "opt1")));

        cut.Find("[role=radio]").Click();
        Assert.Null(received);
    }

    [Fact]
    public void ReadOnlyGroup_PreventsSelection()
    {
        string? received = null;
        var cut = Render<RadioGroupRoot>(p => p
            .Add(c => c.ReadOnly, true)
            .Add(c => c.ValueChanged, (string? v) => received = v)
            .AddChildContent<RadioRoot>(rp => rp.Add(r => r.Value, "opt1")));

        cut.Find("[role=radio]").Click();
        Assert.Null(received);
    }

    [Fact]
    public void HiddenInput_PresentsValue()
    {
        var cut = Render<RadioGroupRoot>(p => p
            .Add(c => c.Value, "opt1")
            .AddChildContent<RadioRoot>(rp => rp.Add(r => r.Value, "opt1")));

        var input = cut.Find("input[type=radio]");
        Assert.Equal("opt1", input.GetAttribute("value"));
        Assert.True(input.HasAttribute("checked"));
    }

    [Fact]
    public void HiddenInput_HasName_WhenGroupNameSet()
    {
        var cut = Render<RadioGroupRoot>(p => p
            .Add(c => c.Name, "my-group")
            .AddChildContent<RadioRoot>(rp => rp.Add(r => r.Value, "opt1")));

        Assert.Equal("my-group", cut.Find("input[type=radio]").GetAttribute("name"));
    }

    // --- RadioIndicator ---

    [Fact]
    public void Indicator_NotRendered_WhenUnchecked()
    {
        var cut = Render<RadioGroupRoot>(p => p
            .AddChildContent<RadioRoot>(rp => rp
                .Add(r => r.Value, "opt1")
                .AddChildContent<RadioIndicator>()));

        // Indicator span should not be present (radio is unchecked)
        Assert.Empty(cut.FindAll("span span"));
    }

    [Fact]
    public void Indicator_Rendered_WhenChecked()
    {
        var cut = Render<RadioGroupRoot>(p => p
            .Add(c => c.Value, "opt1")
            .AddChildContent<RadioRoot>(rp => rp
                .Add(r => r.Value, "opt1")
                .AddChildContent<RadioIndicator>()));

        // Indicator span should be present (radio is checked)
        Assert.NotEmpty(cut.FindAll("span span"));
    }

    [Fact]
    public void Indicator_KeepMounted_RendersWhenUnchecked()
    {
        var cut = Render<RadioGroupRoot>(p => p
            .AddChildContent<RadioRoot>(rp => rp
                .Add(r => r.Value, "opt1")
                .AddChildContent<RadioIndicator>(ip => ip.Add(i => i.KeepMounted, true))));

        Assert.NotEmpty(cut.FindAll("span span"));
    }

    [Fact]
    public void Indicator_Checked_HasDataChecked()
    {
        var cut = Render<RadioGroupRoot>(p => p
            .Add(c => c.Value, "opt1")
            .AddChildContent<RadioRoot>(rp => rp
                .Add(r => r.Value, "opt1")
                .AddChildContent<RadioIndicator>()));

        var indicator = cut.Find("span span");
        Assert.Equal("", indicator.GetAttribute("data-checked"));
        Assert.Null(indicator.GetAttribute("data-unchecked"));
    }

    [Fact]
    public void Indicator_ReadOnly_HasDataReadonly()
    {
        var cut = Render<RadioGroupRoot>(p => p
            .Add(c => c.Value, "opt1")
            .Add(c => c.ReadOnly, true)
            .AddChildContent<RadioRoot>(rp => rp
                .Add(r => r.Value, "opt1")
                .AddChildContent<RadioIndicator>()));

        Assert.Equal("", cut.Find("span span").GetAttribute("data-readonly"));
    }

    [Fact]
    public void Indicator_Required_HasDataRequired()
    {
        var cut = Render<RadioGroupRoot>(p => p
            .Add(c => c.Value, "opt1")
            .Add(c => c.Required, true)
            .AddChildContent<RadioRoot>(rp => rp
                .Add(r => r.Value, "opt1")
                .AddChildContent<RadioIndicator>()));

        Assert.Equal("", cut.Find("span span").GetAttribute("data-required"));
    }
}
