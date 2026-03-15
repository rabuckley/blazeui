using BlazeUI.Headless.Components.Switch;
using Bunit;

namespace BlazeUI.Headless.Tests.Components.Switch;

public class SwitchTests : BunitContext
{
    [Fact]
    public void RendersDefaultSpanTag()
    {
        var cut = Render<SwitchRoot>();
        Assert.Equal("SPAN", cut.Find("[role=switch]").TagName);
    }

    [Fact]
    public void HasSwitchRole()
    {
        var cut = Render<SwitchRoot>();
        Assert.Equal("switch", cut.Find("[role=switch]").GetAttribute("role"));
    }

    [Fact]
    public void AriaCheckedFalseByDefault()
    {
        var cut = Render<SwitchRoot>();
        Assert.Equal("false", cut.Find("[role=switch]").GetAttribute("aria-checked"));
    }

    [Fact]
    public void ClickTogglesChecked()
    {
        var cut = Render<SwitchRoot>();
        cut.Find("[role=switch]").Click();

        Assert.Equal("true", cut.Find("[role=switch]").GetAttribute("aria-checked"));
        Assert.NotNull(cut.Find("[data-checked]"));
        Assert.Empty(cut.FindAll("[data-unchecked]"));
    }

    [Fact]
    public void DataUncheckedPresentWhenNotChecked()
    {
        var cut = Render<SwitchRoot>();
        Assert.NotNull(cut.Find("[data-unchecked]"));
        Assert.Empty(cut.FindAll("[data-checked]"));
    }

    [Fact]
    public void DisabledPreventsToggle()
    {
        var cut = Render<SwitchRoot>(p => p.Add(c => c.Disabled, true));
        cut.Find("[role=switch]").Click();
        Assert.Equal("false", cut.Find("[role=switch]").GetAttribute("aria-checked"));
    }

    [Fact]
    public void DisabledSetsAriaDisabled()
    {
        var cut = Render<SwitchRoot>(p => p.Add(c => c.Disabled, true));
        Assert.Equal("true", cut.Find("[role=switch]").GetAttribute("aria-disabled"));
    }

    [Fact]
    public void DisabledSetsNegativeTabIndex()
    {
        var cut = Render<SwitchRoot>(p => p.Add(c => c.Disabled, true));
        Assert.Equal("-1", cut.Find("[role=switch]").GetAttribute("tabindex"));
    }

    [Fact]
    public void ReadOnlyPreventsToggle()
    {
        var cut = Render<SwitchRoot>(p => p.Add(c => c.ReadOnly, true));
        cut.Find("[role=switch]").Click();
        Assert.Equal("false", cut.Find("[role=switch]").GetAttribute("aria-checked"));
    }

    [Fact]
    public void ReadOnlySetsAriaReadOnly()
    {
        var cut = Render<SwitchRoot>(p => p.Add(c => c.ReadOnly, true));
        Assert.Equal("true", cut.Find("[role=switch]").GetAttribute("aria-readonly"));
    }

    [Fact]
    public void RequiredSetsAriaRequired()
    {
        var cut = Render<SwitchRoot>(p => p.Add(c => c.Required, true));
        Assert.Equal("true", cut.Find("[role=switch]").GetAttribute("aria-required"));
    }

    [Fact]
    public void RequiredSetsDataRequired()
    {
        var cut = Render<SwitchRoot>(p => p.Add(c => c.Required, true));
        Assert.NotNull(cut.Find("[data-required]"));
    }

    [Fact]
    public void ControlledModeReflectsExternalValue()
    {
        var cut = Render<SwitchRoot>(p => p.Add(c => c.Checked, true));
        Assert.Equal("true", cut.Find("[role=switch]").GetAttribute("aria-checked"));
    }

    [Fact]
    public void CheckedChangedFiresOnClick()
    {
        bool? received = null;
        var cut = Render<SwitchRoot>(p => p
            .Add(c => c.CheckedChanged, (bool v) => received = v));
        cut.Find("[role=switch]").Click();
        Assert.True(received);
    }

    [Fact]
    public void DefaultCheckedSetsInitialState()
    {
        var cut = Render<SwitchRoot>(p => p.Add(c => c.DefaultChecked, true));
        Assert.Equal("true", cut.Find("[role=switch]").GetAttribute("aria-checked"));
    }

    [Fact]
    public void HasTabIndexForKeyboardFocusability()
    {
        var cut = Render<SwitchRoot>();
        Assert.Equal("0", cut.Find("[role=switch]").GetAttribute("tabindex"));
    }

    // Hidden input tests

    [Fact]
    public void RendersHiddenInputForFormParticipation()
    {
        var cut = Render<SwitchRoot>();
        var input = cut.Find("input[type=checkbox]");

        Assert.Equal("-1", input.GetAttribute("tabindex"));
        Assert.Equal("true", input.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void HiddenInputReflectsCheckedState()
    {
        var cut = Render<SwitchRoot>(p => p.Add(c => c.DefaultChecked, true));
        var input = cut.Find("input[type=checkbox]");

        Assert.True(input.HasAttribute("checked"));
    }

    [Fact]
    public void NameAppliedToHiddenInputNotVisibleElement()
    {
        var cut = Render<SwitchRoot>(p => p.Add(c => c.Name, "notifications"));

        var input = cut.Find("input[type=checkbox]");
        Assert.Equal("notifications", input.GetAttribute("name"));

        // The visible switch element should not carry the name attribute.
        Assert.Null(cut.Find("[role=switch]").GetAttribute("name"));
    }

    [Fact]
    public void ValueAppliedToHiddenInput()
    {
        var cut = Render<SwitchRoot>(p => p.Add(c => c.Value, "yes"));
        Assert.Equal("yes", cut.Find("input[type=checkbox]").GetAttribute("value"));
    }

    [Fact]
    public void UserProvidedIdRoutedToHiddenInput()
    {
        var cut = Render<SwitchRoot>(p => p.Add(c => c.Id, "my-switch"));

        var input = cut.Find("input[type=checkbox]");
        Assert.Equal("my-switch", input.GetAttribute("id"));

        // The visible element should use an auto-generated id, not the user-provided one.
        var switchEl = cut.Find("[role=switch]");
        Assert.NotEqual("my-switch", switchEl.GetAttribute("id"));
    }

    [Fact]
    public void UncheckedValueRendersHiddenTextInputWhenUnchecked()
    {
        var cut = Render<SwitchRoot>(p => p
            .Add(c => c.Name, "enabled")
            .Add(c => c.UncheckedValue, "off"));

        // When unchecked, there should be a hidden text input carrying the unchecked value.
        var hiddenInput = cut.Find("input[type=hidden]");
        Assert.Equal("off", hiddenInput.GetAttribute("value"));
        Assert.Equal("enabled", hiddenInput.GetAttribute("name"));
    }

    [Fact]
    public void UncheckedValueHiddenInputAbsentWhenChecked()
    {
        var cut = Render<SwitchRoot>(p => p
            .Add(c => c.DefaultChecked, true)
            .Add(c => c.UncheckedValue, "off"));

        // When checked, the unchecked-value input must not appear.
        Assert.Empty(cut.FindAll("input[type=hidden]"));
    }

    // SwitchThumb tests

    [Fact]
    public void ThumbRendersSpanTag()
    {
        var cut = Render<SwitchRoot>(p =>
            p.AddChildContent<SwitchThumb>());

        Assert.NotEmpty(cut.FindAll("span span"));
    }

    [Fact]
    public void ThumbReceivesDataAttributes()
    {
        var cut = Render<SwitchRoot>(p => p
            .Add(c => c.DefaultChecked, true)
            .AddChildContent<SwitchThumb>());

        // The thumb's span (nested inside the root span) should carry data-checked.
        var thumbSpan = cut.FindAll("span")[1];
        Assert.True(thumbSpan.HasAttribute("data-checked"));
    }

    [Fact]
    public void ThumbReceivesDataRequiredFromContext()
    {
        var cut = Render<SwitchRoot>(p => p
            .Add(c => c.Required, true)
            .AddChildContent<SwitchThumb>());

        var thumbSpan = cut.FindAll("span")[1];
        Assert.True(thumbSpan.HasAttribute("data-required"));
    }
}
