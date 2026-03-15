using BlazeUI.Headless.Components.Checkbox;
using Bunit;

namespace BlazeUI.Headless.Tests.Components.Checkbox;

public class CheckboxTests : BunitContext
{
    [Fact]
    public void RendersDefaultSpanTag()
    {
        var cut = Render<CheckboxRoot>();
        Assert.Equal("SPAN", cut.Find("[role=checkbox]").TagName);
    }

    [Fact]
    public void HasCheckboxRole()
    {
        var cut = Render<CheckboxRoot>();
        Assert.Equal("checkbox", cut.Find("[role=checkbox]").GetAttribute("role"));
    }

    [Fact]
    public void AriaCheckedFalseByDefault()
    {
        var cut = Render<CheckboxRoot>();
        Assert.Equal("false", cut.Find("[role=checkbox]").GetAttribute("aria-checked"));
    }

    [Fact]
    public void ClickTogglesChecked()
    {
        var cut = Render<CheckboxRoot>();
        cut.Find("[role=checkbox]").Click();

        Assert.Equal("true", cut.Find("[role=checkbox]").GetAttribute("aria-checked"));
        Assert.NotNull(cut.Find("[data-checked]"));
    }

    [Fact]
    public void DataUncheckedPresentWhenNotChecked()
    {
        var cut = Render<CheckboxRoot>();
        Assert.NotNull(cut.Find("[data-unchecked]"));
    }

    [Fact]
    public void IndeterminateState()
    {
        var cut = Render<CheckboxRoot>(p => p.Add(c => c.Indeterminate, true));

        Assert.Equal("mixed", cut.Find("[role=checkbox]").GetAttribute("aria-checked"));
        Assert.NotNull(cut.Find("[data-indeterminate]"));
        // data-checked and data-unchecked are both suppressed when indeterminate
        Assert.Empty(cut.FindAll("[data-unchecked]"));
        Assert.Empty(cut.FindAll("[data-checked]"));
    }

    [Fact]
    public void DisabledPreventsToggle()
    {
        var cut = Render<CheckboxRoot>(p => p.Add(c => c.Disabled, true));
        cut.Find("[role=checkbox]").Click();
        Assert.Equal("false", cut.Find("[role=checkbox]").GetAttribute("aria-checked"));
    }

    [Fact]
    public void ReadOnlyPreventsToggle()
    {
        var cut = Render<CheckboxRoot>(p => p.Add(c => c.ReadOnly, true));
        cut.Find("[role=checkbox]").Click();
        Assert.Equal("false", cut.Find("[role=checkbox]").GetAttribute("aria-checked"));
    }

    [Fact]
    public void ControlledModeReflectsExternalValue()
    {
        var cut = Render<CheckboxRoot>(p => p.Add(c => c.Checked, true));
        Assert.Equal("true", cut.Find("[role=checkbox]").GetAttribute("aria-checked"));
    }

    [Fact]
    public void CheckedChangedFiresOnClick()
    {
        bool? received = null;
        var cut = Render<CheckboxRoot>(p => p
            .Add(c => c.CheckedChanged, (bool v) => received = v));
        cut.Find("[role=checkbox]").Click();
        Assert.True(received);
    }

    [Fact]
    public void DefaultCheckedSetsInitialState()
    {
        var cut = Render<CheckboxRoot>(p => p.Add(c => c.DefaultChecked, true));
        Assert.Equal("true", cut.Find("[role=checkbox]").GetAttribute("aria-checked"));
    }

    [Fact]
    public void RendersHiddenInputForFormParticipation()
    {
        var cut = Render<CheckboxRoot>();
        var input = cut.Find("input[type=checkbox]");

        Assert.Equal("-1", input.GetAttribute("tabindex"));
        Assert.Equal("true", input.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void HiddenInputReflectsCheckedState()
    {
        var cut = Render<CheckboxRoot>(p => p.Add(c => c.DefaultChecked, true));
        var input = cut.Find("input[type=checkbox]");

        Assert.True(input.HasAttribute("checked"));
    }

    [Fact]
    public void UserProvidedIdRoutedToHiddenInput()
    {
        var cut = Render<CheckboxRoot>(p => p.Add(c => c.Id, "my-checkbox"));

        var input = cut.Find("input[type=checkbox]");
        Assert.Equal("my-checkbox", input.GetAttribute("id"));

        // The visible element should use an auto-generated id, not the user-provided one.
        var checkbox = cut.Find("[role=checkbox]");
        Assert.NotEqual("my-checkbox", checkbox.GetAttribute("id"));
    }

    [Fact]
    public void HiddenInputClickTogglesCheckbox()
    {
        var cut = Render<CheckboxRoot>();
        var input = cut.Find("input[type=checkbox]");

        // Simulates what happens when a <label for="..."> is clicked — the browser
        // dispatches a click event on the associated input.
        input.Click();

        Assert.Equal("true", cut.Find("[role=checkbox]").GetAttribute("aria-checked"));
    }

    [Fact]
    public void HasTabIndexForKeyboardFocusability()
    {
        var cut = Render<CheckboxRoot>();
        Assert.Equal("0", cut.Find("[role=checkbox]").GetAttribute("tabindex"));
    }

    [Fact]
    public void DisabledSetsNegativeTabIndex()
    {
        var cut = Render<CheckboxRoot>(p => p.Add(c => c.Disabled, true));
        Assert.Equal("-1", cut.Find("[role=checkbox]").GetAttribute("tabindex"));
    }

    [Fact]
    public void RequiredSetsAriaRequiredAndDataAttribute()
    {
        var cut = Render<CheckboxRoot>(p => p.Add(c => c.Required, true));
        var checkbox = cut.Find("[role=checkbox]");

        Assert.Equal("true", checkbox.GetAttribute("aria-required"));
        Assert.True(checkbox.HasAttribute("data-required"));
    }

    [Fact]
    public void NotRequiredByDefault()
    {
        var cut = Render<CheckboxRoot>();
        var checkbox = cut.Find("[role=checkbox]");

        Assert.Null(checkbox.GetAttribute("aria-required"));
        Assert.False(checkbox.HasAttribute("data-required"));
    }
}

public class CheckboxIndicatorTests : BunitContext
{
    [Fact]
    public void NotRenderedByDefault()
    {
        // Arrange: unchecked checkbox
        var cut = Render(builder =>
        {
            builder.OpenComponent<CheckboxRoot>(0);
            builder.AddComponentParameter(1, "ChildContent",
                (Microsoft.AspNetCore.Components.RenderFragment)(b =>
                {
                    b.OpenComponent<CheckboxIndicator>(0);
                    b.AddAttribute(1, "data-testid", "indicator");
                    b.CloseComponent();
                }));
            builder.CloseComponent();
        });

        Assert.Empty(cut.FindAll("[data-testid=indicator]"));
    }

    [Fact]
    public void RenderedWhenChecked()
    {
        var cut = Render(builder =>
        {
            builder.OpenComponent<CheckboxRoot>(0);
            builder.AddComponentParameter(1, "Checked", (bool?)true);
            builder.AddComponentParameter(2, "ChildContent",
                (Microsoft.AspNetCore.Components.RenderFragment)(b =>
                {
                    b.OpenComponent<CheckboxIndicator>(0);
                    b.AddAttribute(1, "data-testid", "indicator");
                    b.CloseComponent();
                }));
            builder.CloseComponent();
        });

        Assert.NotEmpty(cut.FindAll("[data-testid=indicator]"));
    }

    [Fact]
    public void RenderedWhenIndeterminate()
    {
        var cut = Render(builder =>
        {
            builder.OpenComponent<CheckboxRoot>(0);
            builder.AddComponentParameter(1, "Indeterminate", true);
            builder.AddComponentParameter(2, "ChildContent",
                (Microsoft.AspNetCore.Components.RenderFragment)(b =>
                {
                    b.OpenComponent<CheckboxIndicator>(0);
                    b.AddAttribute(1, "data-testid", "indicator");
                    b.CloseComponent();
                }));
            builder.CloseComponent();
        });

        Assert.NotEmpty(cut.FindAll("[data-testid=indicator]"));
    }

    [Fact]
    public void KeepMountedRendersWhenUnchecked()
    {
        var cut = Render(builder =>
        {
            builder.OpenComponent<CheckboxRoot>(0);
            builder.AddComponentParameter(2, "ChildContent",
                (Microsoft.AspNetCore.Components.RenderFragment)(b =>
                {
                    b.OpenComponent<CheckboxIndicator>(0);
                    b.AddAttribute(1, "KeepMounted", true);
                    b.AddAttribute(2, "data-testid", "indicator");
                    b.CloseComponent();
                }));
            builder.CloseComponent();
        });

        Assert.NotEmpty(cut.FindAll("[data-testid=indicator]"));
    }

    [Fact]
    public void IndicatorReflectsDataAttributesFromRootState()
    {
        var cut = Render(builder =>
        {
            builder.OpenComponent<CheckboxRoot>(0);
            builder.AddComponentParameter(1, "Checked", (bool?)true);
            builder.AddComponentParameter(2, "Required", true);
            builder.AddComponentParameter(3, "ChildContent",
                (Microsoft.AspNetCore.Components.RenderFragment)(b =>
                {
                    b.OpenComponent<CheckboxIndicator>(0);
                    b.AddAttribute(1, "data-testid", "indicator");
                    b.CloseComponent();
                }));
            builder.CloseComponent();
        });

        var indicator = cut.Find("[data-testid=indicator]");
        Assert.True(indicator.HasAttribute("data-checked"));
        Assert.True(indicator.HasAttribute("data-required"));
    }
}
