using BlazeUI.Headless.Components.NumberField;
using BlazeUI.Headless.Core;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Headless.Tests.Components.NumberField;

public class NumberFieldTests : BunitContext
{
    public NumberFieldTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazeUI();
    }

    [Fact]
    public void RendersDefaultDivTag()
    {
        var cut = Render<NumberFieldRoot>();
        Assert.Equal("DIV", cut.Find("div").TagName);
    }

    [Fact]
    public void DisabledSetsDataAttribute()
    {
        var cut = Render<NumberFieldRoot>(p => p.Add(c => c.Disabled, true));
        Assert.NotNull(cut.Find("[data-disabled]"));
    }

    [Fact]
    public void NotDisabledOmitsDataAttribute()
    {
        var cut = Render<NumberFieldRoot>();
        Assert.Empty(cut.FindAll("[data-disabled]"));
    }

    [Fact]
    public void ReadOnlySetsDataAttribute()
    {
        var cut = Render<NumberFieldRoot>(p => p.Add(c => c.ReadOnly, true));
        Assert.NotNull(cut.Find("[data-readonly]"));
    }

    [Fact]
    public void NotReadOnlyOmitsDataAttribute()
    {
        var cut = Render<NumberFieldRoot>();
        Assert.Empty(cut.FindAll("[data-readonly]"));
    }

    [Fact]
    public void RequiredSetsDataAttribute()
    {
        var cut = Render<NumberFieldRoot>(p => p.Add(c => c.Required, true));
        Assert.NotNull(cut.Find("[data-required]"));
    }

    [Fact]
    public void NotRequiredOmitsDataAttribute()
    {
        var cut = Render<NumberFieldRoot>();
        Assert.Empty(cut.FindAll("[data-required]"));
    }

    [Fact]
    public void NotScrubbingOmitsDataAttribute()
    {
        // data-scrubbing is only present when a scrub gesture is active.
        var cut = Render<NumberFieldRoot>();
        Assert.Empty(cut.FindAll("[data-scrubbing]"));
    }

    [Fact]
    public void ControlledValueRendersWithoutError()
    {
        var cut = Render<NumberFieldRoot>(p => p.Add(c => c.Value, 10.0));
        Assert.NotNull(cut.Find("div"));
    }

    [Fact]
    public void DefaultValueRendersWithoutError()
    {
        var cut = Render<NumberFieldRoot>(p => p.Add(c => c.DefaultValue, 42.0));
        Assert.NotNull(cut.Find("div"));
    }

    // -- NumberFieldInput --

    [Fact]
    public void InputChildRendersInputElement()
    {
        var cut = Render<NumberFieldRoot>(p => p
            .AddChildContent<NumberFieldInput>());

        Assert.Equal("INPUT", cut.Find("input").TagName);
    }

    [Fact]
    public void InputHasDecimalInputMode()
    {
        var cut = Render<NumberFieldRoot>(p => p
            .AddChildContent<NumberFieldInput>());

        Assert.Equal("decimal", cut.Find("input").GetAttribute("inputmode"));
    }

    [Fact]
    public void InputHasAriaRoleDescription()
    {
        // Base UI sets aria-roledescription="Number field" on the visible input so
        // screen readers announce the field type without a numeric spinbutton role.
        var cut = Render<NumberFieldRoot>(p => p
            .AddChildContent<NumberFieldInput>());

        Assert.Equal("Number field", cut.Find("input").GetAttribute("aria-roledescription"));
    }

    [Fact]
    public void InputReflectsDisabledState()
    {
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.Disabled, true)
            .AddChildContent<NumberFieldInput>());

        Assert.True(cut.Find("input").HasAttribute("disabled"));
    }

    [Fact]
    public void InputReflectsReadOnlyState()
    {
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.ReadOnly, true)
            .AddChildContent<NumberFieldInput>());

        Assert.True(cut.Find("input").HasAttribute("readonly"));
    }

    [Fact]
    public void InputReflectsRequiredState()
    {
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.Required, true)
            .AddChildContent<NumberFieldInput>());

        Assert.True(cut.Find("input").HasAttribute("required"));
    }

    [Fact]
    public void InputShowsControlledValue()
    {
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.Value, 25.5)
            .AddChildContent<NumberFieldInput>());

        Assert.Equal("25.5", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void InputDataRequiredPropagates()
    {
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.Required, true)
            .AddChildContent<NumberFieldInput>());

        Assert.NotNull(cut.Find("input[data-required]"));
    }

    // -- Keyboard: ArrowUp / ArrowDown --

    [Fact]
    public void ArrowUp_IncrementsValue()
    {
        double? received = null;
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.DefaultValue, 5.0)
            .Add(c => c.ValueChanged, (double? v) => received = v)
            .AddChildContent<NumberFieldInput>());

        cut.Find("input").KeyDown(new KeyboardEventArgs { Key = "ArrowUp" });
        Assert.Equal(6.0, received);
    }

    [Fact]
    public void ArrowDown_DecrementsValue()
    {
        double? received = null;
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.DefaultValue, 5.0)
            .Add(c => c.ValueChanged, (double? v) => received = v)
            .AddChildContent<NumberFieldInput>());

        cut.Find("input").KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        Assert.Equal(4.0, received);
    }

    [Fact]
    public void ShiftArrowUp_UsesLargeStep()
    {
        double? received = null;
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.DefaultValue, 0.0)
            .Add(c => c.Step, 1.0)
            .Add(c => c.LargeStep, 10.0)
            .Add(c => c.ValueChanged, (double? v) => received = v)
            .AddChildContent<NumberFieldInput>());

        cut.Find("input").KeyDown(new KeyboardEventArgs { Key = "ArrowUp", ShiftKey = true });
        Assert.Equal(10.0, received);
    }

    [Fact]
    public void AltArrowDown_UsesSmallStep()
    {
        double? received = null;
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.DefaultValue, 1.0)
            .Add(c => c.SmallStep, 0.1)
            .Add(c => c.ValueChanged, (double? v) => received = v)
            .AddChildContent<NumberFieldInput>());

        cut.Find("input").KeyDown(new KeyboardEventArgs { Key = "ArrowDown", AltKey = true });
        // 1.0 - 0.1 = 0.9, rounded to avoid floating-point noise in the assertion
        Assert.Equal(0.9, received!.Value, precision: 10);
    }

    [Fact]
    public void HomeKey_SetsValueToMin()
    {
        double? received = null;
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.DefaultValue, 50.0)
            .Add(c => c.Min, 0.0)
            .Add(c => c.ValueChanged, (double? v) => received = v)
            .AddChildContent<NumberFieldInput>());

        cut.Find("input").KeyDown(new KeyboardEventArgs { Key = "Home" });
        Assert.Equal(0.0, received);
    }

    [Fact]
    public void EndKey_SetsValueToMax()
    {
        double? received = null;
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.DefaultValue, 50.0)
            .Add(c => c.Max, 100.0)
            .Add(c => c.ValueChanged, (double? v) => received = v)
            .AddChildContent<NumberFieldInput>());

        cut.Find("input").KeyDown(new KeyboardEventArgs { Key = "End" });
        Assert.Equal(100.0, received);
    }

    [Fact]
    public void HomeKey_NoEffectWithoutMin()
    {
        double? received = null;
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.DefaultValue, 50.0)
            .Add(c => c.ValueChanged, (double? v) => received = v)
            .AddChildContent<NumberFieldInput>());

        cut.Find("input").KeyDown(new KeyboardEventArgs { Key = "Home" });
        Assert.Null(received);
    }

    [Fact]
    public void EndKey_NoEffectWithoutMax()
    {
        double? received = null;
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.DefaultValue, 50.0)
            .Add(c => c.ValueChanged, (double? v) => received = v)
            .AddChildContent<NumberFieldInput>());

        cut.Find("input").KeyDown(new KeyboardEventArgs { Key = "End" });
        Assert.Null(received);
    }

    [Fact]
    public void ArrowKeys_DisabledPreventsChange()
    {
        double? received = null;
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.DefaultValue, 5.0)
            .Add(c => c.Disabled, true)
            .Add(c => c.ValueChanged, (double? v) => received = v)
            .AddChildContent<NumberFieldInput>());

        cut.Find("input").KeyDown(new KeyboardEventArgs { Key = "ArrowUp" });
        Assert.Null(received);
    }

    [Fact]
    public void ArrowKeys_ReadOnlyPreventsChange()
    {
        double? received = null;
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.DefaultValue, 5.0)
            .Add(c => c.ReadOnly, true)
            .Add(c => c.ValueChanged, (double? v) => received = v)
            .AddChildContent<NumberFieldInput>());

        cut.Find("input").KeyDown(new KeyboardEventArgs { Key = "ArrowUp" });
        Assert.Null(received);
    }

    // -- NumberFieldGroup --

    [Fact]
    public void GroupHasGroupRole()
    {
        // Base UI specifies role="group" on NumberFieldGroup so the increment/decrement
        // buttons are announced as belonging to the same number field.
        var cut = Render<NumberFieldRoot>(p => p
            .AddChildContent<NumberFieldGroup>());

        Assert.Equal("group", cut.Find("div > div").GetAttribute("role"));
    }

    [Fact]
    public void GroupRendersDefaultDivTag()
    {
        var cut = Render<NumberFieldRoot>(p => p
            .AddChildContent<NumberFieldGroup>());

        // Root renders a div, Group renders a nested div
        var divs = cut.FindAll("div");
        Assert.True(divs.Count >= 2);
    }

    [Fact]
    public void GroupDataRequiredPropagates()
    {
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.Required, true)
            .AddChildContent<NumberFieldGroup>());

        Assert.NotNull(cut.Find("div[data-required]"));
    }

    [Fact]
    public void GroupDataDisabledPropagates()
    {
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.Disabled, true)
            .AddChildContent<NumberFieldGroup>());

        // Both root and group should carry data-disabled.
        var items = cut.FindAll("[data-disabled]");
        Assert.True(items.Count >= 2);
    }

    // -- NumberFieldIncrement / Decrement --

    [Fact]
    public void IncrementButtonRendersWithAriaLabel()
    {
        var cut = Render<NumberFieldRoot>(p => p
            .AddChildContent<NumberFieldIncrement>(bp => bp
                .AddChildContent("+")));

        var button = cut.Find("button");
        Assert.Equal("Increase", button.GetAttribute("aria-label"));
    }

    [Fact]
    public void DecrementButtonRendersWithAriaLabel()
    {
        var cut = Render<NumberFieldRoot>(p => p
            .AddChildContent<NumberFieldDecrement>(bp => bp
                .AddChildContent("-")));

        var button = cut.Find("button");
        Assert.Equal("Decrease", button.GetAttribute("aria-label"));
    }

    [Fact]
    public void IncrementDisabledAtMaxValue()
    {
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.Value, 10.0)
            .Add(c => c.Max, 10.0)
            .AddChildContent<NumberFieldIncrement>(bp => bp
                .AddChildContent("+")));

        Assert.True(cut.Find("button").HasAttribute("disabled"));
        Assert.NotNull(cut.Find("[data-disabled]"));
    }

    [Fact]
    public void DecrementDisabledAtMinValue()
    {
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.Value, 0.0)
            .Add(c => c.Min, 0.0)
            .AddChildContent<NumberFieldDecrement>(bp => bp
                .AddChildContent("-")));

        Assert.True(cut.Find("button").HasAttribute("disabled"));
        Assert.NotNull(cut.Find("[data-disabled]"));
    }

    [Fact]
    public void IncrementEnabledBelowMax()
    {
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.Value, 5.0)
            .Add(c => c.Max, 10.0)
            .AddChildContent<NumberFieldIncrement>(bp => bp
                .AddChildContent("+")));

        Assert.False(cut.Find("button").HasAttribute("disabled"));
    }

    [Fact]
    public void DecrementEnabledAboveMin()
    {
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.Value, 5.0)
            .Add(c => c.Min, 0.0)
            .AddChildContent<NumberFieldDecrement>(bp => bp
                .AddChildContent("-")));

        Assert.False(cut.Find("button").HasAttribute("disabled"));
    }

    [Fact]
    public void IncrementDataReadOnlyWhenReadOnly()
    {
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.ReadOnly, true)
            .AddChildContent<NumberFieldIncrement>(bp => bp
                .AddChildContent("+")));

        Assert.NotNull(cut.Find("button[data-readonly]"));
    }

    [Fact]
    public void DecrementDataReadOnlyWhenReadOnly()
    {
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.ReadOnly, true)
            .AddChildContent<NumberFieldDecrement>(bp => bp
                .AddChildContent("-")));

        Assert.NotNull(cut.Find("button[data-readonly]"));
    }

    [Fact]
    public void StepperButtonsHaveTabIndexMinusOne()
    {
        var cut = Render<NumberFieldRoot>(p => p
            .AddChildContent<NumberFieldIncrement>(bp => bp
                .AddChildContent("+")));

        Assert.Equal("-1", cut.Find("button").GetAttribute("tabindex"));
    }

    [Fact]
    public void IncrementClickFiresValueChanged()
    {
        double? received = null;
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.DefaultValue, 5.0)
            .Add(c => c.ValueChanged, (double? v) => received = v)
            .AddChildContent<NumberFieldIncrement>(bp => bp
                .AddChildContent("+")));

        cut.Find("button").Click();
        Assert.Equal(6.0, received);
    }

    [Fact]
    public void DecrementClickFiresValueChanged()
    {
        double? received = null;
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.DefaultValue, 5.0)
            .Add(c => c.ValueChanged, (double? v) => received = v)
            .AddChildContent<NumberFieldDecrement>(bp => bp
                .AddChildContent("-")));

        cut.Find("button").Click();
        Assert.Equal(4.0, received);
    }

    [Fact]
    public void IncrementRespectsStepSize()
    {
        double? received = null;
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.DefaultValue, 0.0)
            .Add(c => c.Step, 0.5)
            .Add(c => c.ValueChanged, (double? v) => received = v)
            .AddChildContent<NumberFieldIncrement>(bp => bp
                .AddChildContent("+")));

        cut.Find("button").Click();
        Assert.Equal(0.5, received);
    }

    [Fact]
    public void ValueClampedToMax()
    {
        double? received = null;
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.DefaultValue, 9.0)
            .Add(c => c.Max, 10.0)
            .Add(c => c.Step, 5.0)
            .Add(c => c.ValueChanged, (double? v) => received = v)
            .AddChildContent<NumberFieldIncrement>(bp => bp
                .AddChildContent("+")));

        cut.Find("button").Click();
        Assert.Equal(10.0, received);
    }

    [Fact]
    public void ValueClampedToMin()
    {
        double? received = null;
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.DefaultValue, 1.0)
            .Add(c => c.Min, 0.0)
            .Add(c => c.Step, 5.0)
            .Add(c => c.ValueChanged, (double? v) => received = v)
            .AddChildContent<NumberFieldDecrement>(bp => bp
                .AddChildContent("-")));

        cut.Find("button").Click();
        Assert.Equal(0.0, received);
    }

    [Fact]
    public void DisabledPreventsIncrement()
    {
        double? received = null;
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.DefaultValue, 5.0)
            .Add(c => c.Disabled, true)
            .Add(c => c.ValueChanged, (double? v) => received = v)
            .AddChildContent<NumberFieldIncrement>(bp => bp
                .AddChildContent("+")));

        cut.Find("button").Click();
        Assert.Null(received);
    }

    [Fact]
    public void ReadOnlyPreventsDecrement()
    {
        double? received = null;
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.DefaultValue, 5.0)
            .Add(c => c.ReadOnly, true)
            .Add(c => c.ValueChanged, (double? v) => received = v)
            .AddChildContent<NumberFieldDecrement>(bp => bp
                .AddChildContent("-")));

        cut.Find("button").Click();
        Assert.Null(received);
    }

    /// <summary>
    /// Regression: in controlled mode, StepValue must fire ValueChanged with the
    /// computed new value, not the stale controlled value from ComponentState.Value.
    /// </summary>
    [Fact]
    public void ControlledMode_IncrementFiresCorrectValue()
    {
        double? received = null;
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.Value, 5.0)
            .Add(c => c.Step, 1.0)
            .Add(c => c.ValueChanged, (double? v) => received = v)
            .AddChildContent<NumberFieldIncrement>(bp => bp
                .AddChildContent("+")));

        cut.Find("button").Click();

        Assert.Equal(6.0, received);
    }

    /// <summary>
    /// Regression: in controlled mode, decrement must fire the correct computed value.
    /// </summary>
    [Fact]
    public void ControlledMode_DecrementFiresCorrectValue()
    {
        double? received = null;
        var cut = Render<NumberFieldRoot>(p => p
            .Add(c => c.Value, 5.0)
            .Add(c => c.Step, 1.0)
            .Add(c => c.ValueChanged, (double? v) => received = v)
            .AddChildContent<NumberFieldDecrement>(bp => bp
                .AddChildContent("-")));

        cut.Find("button").Click();

        Assert.Equal(4.0, received);
    }
}
