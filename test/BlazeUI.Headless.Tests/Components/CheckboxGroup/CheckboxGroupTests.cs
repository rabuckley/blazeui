using BlazeUI.Headless.Components.Checkbox;
using BlazeUI.Headless.Components.CheckboxGroup;
using BlazeUI.Headless.Core;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Headless.Tests.Components.CheckboxGroup;

public class CheckboxGroupTests : BunitContext
{
    public CheckboxGroupTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazeUI();
    }

    // --- Root rendering ---

    [Fact]
    public void Root_RendersDiv()
    {
        var cut = Render<CheckboxGroupRoot>();
        Assert.Equal("DIV", cut.Find("div").TagName);
    }

    [Fact]
    public void Root_HasGroupRole()
    {
        var cut = Render<CheckboxGroupRoot>();
        Assert.Equal("group", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void Root_NoDataDisabledByDefault()
    {
        var cut = Render<CheckboxGroupRoot>();
        Assert.Empty(cut.FindAll("[data-disabled]"));
    }

    [Fact]
    public void Root_DataDisabledWhenDisabled()
    {
        var cut = Render<CheckboxGroupRoot>(p => p.Add(c => c.Disabled, true));
        Assert.NotNull(cut.Find("[data-disabled]"));
    }

    [Fact]
    public void Root_AriaLabelledByEmittedWhenProvided()
    {
        var cut = Render<CheckboxGroupRoot>(p => p.Add(c => c.LabelledBy, "my-label"));
        Assert.Equal("my-label", cut.Find("[role=group]").GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void Root_NoAriaLabelledByByDefault()
    {
        var cut = Render<CheckboxGroupRoot>();
        Assert.Null(cut.Find("[role=group]").GetAttribute("aria-labelledby"));
    }

    // --- Child checkbox checked state ---

    [Fact]
    public void ChildCheckbox_CheckedWhenValueInList()
    {
        var cut = Render<CheckboxGroupRoot>(p => p
            .Add(c => c.Value, new[] { "opt1" })
            .AddChildContent<CheckboxRoot>(cp => cp.Add(c => c.Value, "opt1"))
        );

        Assert.Equal("true", cut.Find("[role=checkbox]").GetAttribute("aria-checked"));
    }

    [Fact]
    public void ChildCheckbox_UncheckedWhenValueNotInList()
    {
        var cut = Render<CheckboxGroupRoot>(p => p
            .Add(c => c.Value, new[] { "other" })
            .AddChildContent<CheckboxRoot>(cp => cp.Add(c => c.Value, "opt1"))
        );

        Assert.Equal("false", cut.Find("[role=checkbox]").GetAttribute("aria-checked"));
    }

    // --- Value change ---

    [Fact]
    public void TogglesToChildCheckbox_AddsValue()
    {
        IReadOnlyList<string>? received = null;
        var cut = Render<CheckboxGroupRoot>(p => p
            .Add(c => c.ValueChanged, (IReadOnlyList<string> v) => received = v)
            .AddChildContent<CheckboxRoot>(cp => cp.Add(c => c.Value, "opt1"))
        );

        cut.Find("[role=checkbox]").Click();

        Assert.NotNull(received);
        Assert.Single(received!);
        Assert.Equal("opt1", received![0]);
    }

    [Fact]
    public void TogglesToChildCheckbox_RemovesValue()
    {
        IReadOnlyList<string>? received = null;

        // Start with opt1 checked, then click to uncheck.
        var cut = Render<CheckboxGroupRoot>(p => p
            .Add(c => c.DefaultValue, new[] { "opt1" })
            .Add(c => c.ValueChanged, (IReadOnlyList<string> v) => received = v)
            .AddChildContent<CheckboxRoot>(cp => cp.Add(c => c.Value, "opt1"))
        );

        cut.Find("[role=checkbox]").Click();

        Assert.NotNull(received);
        Assert.Empty(received!);
    }

    [Fact]
    public void ControlledValue_ReflectsExternalChange()
    {
        var cut = Render<CheckboxGroupRoot>(p => p
            .Add(c => c.Value, new[] { "red" })
            .AddChildContent<CheckboxRoot>(cp => cp.Add(c => c.Value, "red"))
        );

        Assert.Equal("true", cut.Find("[role=checkbox]").GetAttribute("aria-checked"));
    }

    [Fact]
    public void DefaultValue_SetsInitialCheckedState()
    {
        var cut = Render<CheckboxGroupRoot>(p => p
            .Add(c => c.DefaultValue, new[] { "opt1" })
            .AddChildContent<CheckboxRoot>(cp => cp.Add(c => c.Value, "opt1"))
        );

        Assert.Equal("true", cut.Find("[role=checkbox]").GetAttribute("aria-checked"));
    }

    // --- Disabled ---

    [Fact]
    public void GroupDisabled_PreventToggle()
    {
        IReadOnlyList<string>? received = null;
        var cut = Render<CheckboxGroupRoot>(p => p
            .Add(c => c.Disabled, true)
            .Add(c => c.ValueChanged, (IReadOnlyList<string> v) => received = v)
            .AddChildContent<CheckboxRoot>(cp => cp.Add(c => c.Value, "opt1"))
        );

        cut.Find("[role=checkbox]").Click();
        Assert.Null(received);
    }

    [Fact]
    public void GroupDisabled_PropagatesAriaDisabledToChildren()
    {
        var cut = Render<CheckboxGroupRoot>(p => p
            .Add(c => c.Disabled, true)
            .AddChildContent(content =>
            {
                content.OpenComponent<CheckboxRoot>(0);
                content.AddComponentParameter(1, nameof(CheckboxRoot.Value), "red");
                content.CloseComponent();

                content.OpenComponent<CheckboxRoot>(2);
                content.AddComponentParameter(3, nameof(CheckboxRoot.Value), "green");
                content.CloseComponent();
            })
        );

        foreach (var checkbox in cut.FindAll("[role=checkbox]"))
            Assert.Equal("true", checkbox.GetAttribute("aria-disabled"));
    }

    [Fact]
    public void GroupDisabled_TakesPrecedenceOverIndividualCheckbox()
    {
        // Even when a child sets Disabled=false explicitly, the group's disabled wins.
        var cut = Render<CheckboxGroupRoot>(p => p
            .Add(c => c.Disabled, true)
            .AddChildContent<CheckboxRoot>(cp => cp
                .Add(c => c.Value, "opt1")
                .Add(c => c.Disabled, false))
        );

        Assert.Equal("true", cut.Find("[role=checkbox]").GetAttribute("aria-disabled"));
    }

    [Fact]
    public void GroupNotDisabled_ChildCheckboxNotAriaDisabled()
    {
        var cut = Render<CheckboxGroupRoot>(p => p
            .Add(c => c.Disabled, false)
            .AddChildContent<CheckboxRoot>(cp => cp.Add(c => c.Value, "opt1"))
        );

        Assert.Null(cut.Find("[role=checkbox]").GetAttribute("aria-disabled"));
    }

    // --- Multiple values ---

    [Fact]
    public void MultipleCheckboxes_IndependentToggle()
    {
        IReadOnlyList<string>? received = null;
        var cut = Render<CheckboxGroupRoot>(p => p
            .Add(c => c.ValueChanged, (IReadOnlyList<string> v) => received = v)
            .AddChildContent(content =>
            {
                content.OpenComponent<CheckboxRoot>(0);
                content.AddComponentParameter(1, nameof(CheckboxRoot.Value), "red");
                content.CloseComponent();

                content.OpenComponent<CheckboxRoot>(2);
                content.AddComponentParameter(3, nameof(CheckboxRoot.Value), "green");
                content.CloseComponent();
            })
        );

        var checkboxes = cut.FindAll("[role=checkbox]");

        checkboxes[0].Click();
        Assert.Equal(["red"], received);

        // After the callback fires, re-query since checkboxes list may have been updated.
        cut.FindAll("[role=checkbox]")[1].Click();
        Assert.Equal(["red", "green"], received);
    }
}
