using BlazeUI.Headless.Components.Select;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Tests.Components.Select;

public class SelectSubPartTests : BunitContext
{
    private SelectContext CreateContext(bool open = false, string? selectedValue = null) => new()
    {
        Open = open,
        SelectedValue = selectedValue,
        TriggerId = "test-trigger",
        PopupId = "test-popup",
    };

    // -- SelectLabel --

    [Fact]
    public void SelectLabel_RendersDiv()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectLabel>(lp => lp.AddChildContent("Fruit")));

        Assert.Equal("DIV", cut.Find("div").TagName);
    }

    [Fact]
    public void SelectLabel_WiresIdToContext()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectLabel>(lp => lp.AddChildContent("Fruit")));

        Assert.NotNull(ctx.LabelId);
        Assert.Contains("select-label-", ctx.LabelId);
    }

    [Fact]
    public void SelectLabel_DisabledSetsDataAttribute()
    {
        var ctx = CreateContext();
        ctx.Disabled = true;
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectLabel>(lp => lp.AddChildContent("Fruit")));

        Assert.NotNull(cut.Find("[data-disabled]"));
    }

    [Fact]
    public void SelectLabel_NotDisabledOmitsDataAttribute()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectLabel>(lp => lp.AddChildContent("Fruit")));

        Assert.Empty(cut.FindAll("[data-disabled]"));
    }

    // -- SelectList --

    [Fact]
    public void SelectList_HasListboxRole()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectList>(lp => lp.AddChildContent("items")));

        Assert.Equal("listbox", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void SelectList_WiresIdToContext()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectList>(lp => lp.AddChildContent("items")));

        Assert.NotNull(ctx.ListId);
        Assert.Contains("select-list-", ctx.ListId);
    }

    [Fact]
    public void SelectList_AriaLabelledByDefaultsToTriggerId()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectList>(lp => lp.AddChildContent("items")));

        Assert.Equal("test-trigger", cut.Find("div").GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void SelectList_AriaLabelledByUsesLabelIdWhenPresent()
    {
        var ctx = CreateContext(open: true);
        ctx.LabelId = "my-label-id";
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectList>(lp => lp.AddChildContent("items")));

        Assert.Equal("my-label-id", cut.Find("div").GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void SelectList_HasDataOpenWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectList>(lp => lp.AddChildContent("items")));

        Assert.NotNull(cut.Find("[data-open]"));
    }

    [Fact]
    public void SelectList_HasDataClosedWhenClosed()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectList>(lp => lp.AddChildContent("items")));

        Assert.NotNull(cut.Find("[data-closed]"));
    }

    // -- SelectTrigger with LabelId --

    [Fact]
    public void Trigger_HasAriaLabelledByWhenLabelPresent()
    {
        var ctx = CreateContext();
        ctx.LabelId = "my-label";
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectTrigger>(tp => tp.AddChildContent("Select")));

        Assert.Equal("my-label", cut.Find("button").GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void Trigger_NoAriaLabelledByWhenNoLabel()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectTrigger>(tp => tp.AddChildContent("Select")));

        Assert.Null(cut.Find("button").GetAttribute("aria-labelledby"));
    }

    // -- SelectGroup with label association --

    [Fact]
    public void Group_HasGroupRole()
    {
        var cut = Render<SelectGroup>();
        Assert.Equal("group", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void Group_HasAriaLabelledByFromGroupLabel()
    {
        var cut = Render<SelectGroup>(p => p
            .AddChildContent<SelectGroupLabel>(lp => lp.AddChildContent("Fruits")));

        var group = cut.Find("[role='group']");
        Assert.NotNull(group.GetAttribute("aria-labelledby"));
        var labelId = group.GetAttribute("aria-labelledby");
        Assert.NotNull(cut.Find($"#{labelId}"));
    }

    // -- SelectScrollUpArrow --

    [Fact]
    public void ScrollUpArrow_HasDirectionDataAttribute()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectScrollUpArrow>(sp => sp
                .Add(c => c.KeepMounted, true)
                .AddChildContent("▲")));

        Assert.Equal("up", cut.Find("div").GetAttribute("data-direction"));
    }

    [Fact]
    public void ScrollUpArrow_HasAriaHidden()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectScrollUpArrow>(sp => sp
                .Add(c => c.KeepMounted, true)
                .AddChildContent("▲")));

        Assert.Equal("true", cut.Find("div").GetAttribute("aria-hidden"));
    }

    [Fact]
    public void ScrollUpArrow_NotVisibleByDefault()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectScrollUpArrow>(sp => sp
                .Add(c => c.KeepMounted, true)
                .AddChildContent("▲")));

        // data-visible is not set when not scrollable.
        Assert.Empty(cut.FindAll("[data-visible]"));
    }

    [Fact]
    public void ScrollUpArrow_HiddenWhenNotMountedAndNotVisible()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectScrollUpArrow>(sp => sp
                .AddChildContent("▲")));

        // Without KeepMounted, the arrow should not render when not visible.
        Assert.Empty(cut.FindAll("[data-direction]"));
    }

    // -- SelectScrollDownArrow --

    [Fact]
    public void ScrollDownArrow_HasDirectionDataAttribute()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectScrollDownArrow>(sp => sp
                .Add(c => c.KeepMounted, true)
                .AddChildContent("▼")));

        Assert.Equal("down", cut.Find("div").GetAttribute("data-direction"));
    }

    [Fact]
    public void ScrollDownArrow_HasAriaHidden()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectScrollDownArrow>(sp => sp
                .Add(c => c.KeepMounted, true)
                .AddChildContent("▼")));

        Assert.Equal("true", cut.Find("div").GetAttribute("aria-hidden"));
    }

    // -- SelectArrow --

    [Fact]
    public void Arrow_HasAriaHidden()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectArrow>());

        Assert.Equal("true", cut.Find("div").GetAttribute("aria-hidden"));
    }

    [Fact]
    public void Arrow_HasDataOpenWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectArrow>());

        Assert.NotNull(cut.Find("[data-open]"));
    }

    [Fact]
    public void Arrow_HasDataClosedWhenClosed()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectArrow>());

        Assert.NotNull(cut.Find("[data-closed]"));
    }

    // -- SelectIcon --

    [Fact]
    public void Icon_HasAriaHidden()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectIcon>());

        Assert.Equal("true", cut.Find("span").GetAttribute("aria-hidden"));
    }

    [Fact]
    public void Icon_HasDataPopupOpenWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<SelectContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<SelectIcon>());

        Assert.NotNull(cut.Find("[data-popup-open]"));
    }
}
