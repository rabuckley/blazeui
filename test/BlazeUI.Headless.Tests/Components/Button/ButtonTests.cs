using Bunit;
using ButtonComponent = BlazeUI.Headless.Components.Button.Button;

namespace BlazeUI.Headless.Tests.Components.Button;

public sealed class ButtonTests : BunitContext
{
    [Fact]
    public void Renders_default_button_tag()
    {
        var cut = Render<ButtonComponent>();

        Assert.NotNull(cut.Find("button"));
    }

    [Fact]
    public void As_parameter_overrides_tag()
    {
        var cut = Render<ButtonComponent>(p => p.Add(c => c.As, "span"));

        Assert.NotNull(cut.Find("span"));
    }

    // --- data-disabled ---

    [Fact]
    public void Data_disabled_present_when_disabled()
    {
        var cut = Render<ButtonComponent>(p => p.Add(c => c.Disabled, true));

        Assert.NotNull(cut.Find("[data-disabled]"));
    }

    [Fact]
    public void Data_disabled_absent_when_not_disabled()
    {
        var cut = Render<ButtonComponent>();

        Assert.Empty(cut.FindAll("[data-disabled]"));
    }

    // --- type="button" ---

    [Fact]
    public void Native_button_has_type_button()
    {
        // Prevents accidental form submission when the button is inside a <form>.
        var cut = Render<ButtonComponent>();
        var element = cut.Find("button");

        Assert.Equal("button", element.GetAttribute("type"));
    }

    [Fact]
    public void Non_native_element_has_no_type_attribute()
    {
        var cut = Render<ButtonComponent>(p => p.Add(c => c.As, "div"));
        var element = cut.Find("div");

        Assert.False(element.HasAttribute("type"));
    }

    // --- Native button disabled ---

    [Fact]
    public void Native_disabled_when_disabled_and_not_focusable()
    {
        var cut = Render<ButtonComponent>(p => p.Add(c => c.Disabled, true));
        var element = cut.Find("button");

        Assert.True(element.HasAttribute("disabled"));
        Assert.False(element.HasAttribute("aria-disabled"));
    }

    // --- Native button + focusableWhenDisabled ---

    [Fact]
    public void Native_button_focusable_when_disabled_uses_aria_disabled_not_disabled()
    {
        var cut = Render<ButtonComponent>(p => p
            .Add(c => c.Disabled, true)
            .Add(c => c.FocusableWhenDisabled, true));
        var element = cut.Find("button");

        // aria-disabled keeps the element in the tab order; native disabled would remove it.
        Assert.Equal("true", element.GetAttribute("aria-disabled"));
        Assert.False(element.HasAttribute("disabled"));
    }

    // --- Non-native button role and tabindex ---

    [Fact]
    public void Non_native_button_has_role_button_and_tabindex_zero()
    {
        var cut = Render<ButtonComponent>(p => p.Add(c => c.As, "div"));
        var element = cut.Find("div");

        Assert.Equal("button", element.GetAttribute("role"));
        Assert.Equal("0", element.GetAttribute("tabindex"));
    }

    // --- Non-native button disabled ---

    [Fact]
    public void Non_native_disabled_has_aria_disabled_and_negative_tabindex()
    {
        // A disabled non-native button uses aria-disabled rather than the native attribute
        // and tabindex="-1" to remove it from the focus order.
        var cut = Render<ButtonComponent>(p => p
            .Add(c => c.As, "div")
            .Add(c => c.Disabled, true));
        var element = cut.Find("div");

        Assert.Equal("button", element.GetAttribute("role"));
        Assert.Equal("true", element.GetAttribute("aria-disabled"));
        Assert.Equal("-1", element.GetAttribute("tabindex"));
        Assert.False(element.HasAttribute("disabled"));
    }

    // --- Non-native button + focusableWhenDisabled ---

    [Fact]
    public void Non_native_disabled_focusable_has_aria_disabled_and_tabindex_zero()
    {
        var cut = Render<ButtonComponent>(p => p
            .Add(c => c.As, "div")
            .Add(c => c.Disabled, true)
            .Add(c => c.FocusableWhenDisabled, true));
        var element = cut.Find("div");

        Assert.Equal("true", element.GetAttribute("aria-disabled"));
        Assert.Equal("0", element.GetAttribute("tabindex"));
        Assert.False(element.HasAttribute("disabled"));
    }
}
