using BlazeUI.Headless.Components.Toolbar;
using BlazeUI.Headless.Core;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Headless.Tests.Components.Toolbar;

public class ToolbarTests : BunitContext
{
    public ToolbarTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazeUI();
    }

    // --- Root ---

    [Fact]
    public void Root_HasToolbarRole()
    {
        var cut = Render<ToolbarRoot>();
        Assert.Equal("toolbar", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void Root_HasHorizontalOrientationByDefault()
    {
        var cut = Render<ToolbarRoot>();
        Assert.Equal("horizontal", cut.Find("div").GetAttribute("aria-orientation"));
        Assert.Equal("horizontal", cut.Find("div").GetAttribute("data-orientation"));
    }

    [Fact]
    public void Root_VerticalOrientation_SetsAriaAndDataOrientation()
    {
        var cut = Render<ToolbarRoot>(p => p.Add(c => c.Orientation, Orientation.Vertical));
        Assert.Equal("vertical", cut.Find("div").GetAttribute("aria-orientation"));
        Assert.Equal("vertical", cut.Find("div").GetAttribute("data-orientation"));
    }

    [Fact]
    public void Root_Disabled_SetsDataDisabled()
    {
        var cut = Render<ToolbarRoot>(p => p.Add(c => c.Disabled, true));
        Assert.NotNull(cut.Find("[data-disabled]"));
    }

    [Fact]
    public void Root_NotDisabled_OmitsDataDisabled()
    {
        var cut = Render<ToolbarRoot>();
        Assert.Null(cut.Find("div").GetAttribute("data-disabled"));
    }

    // --- Separator ---

    [Fact]
    public void Separator_HasSeparatorRole()
    {
        var cut = RenderWithRoot((RenderFragment)(b =>
        {
            b.OpenComponent<ToolbarSeparator>(0);
            b.CloseComponent();
        }));

        Assert.NotNull(cut.Find("[role='separator']"));
    }

    [Fact]
    public void Separator_IsPerpendicularToHorizontalToolbar()
    {
        var cut = RenderWithRoot((RenderFragment)(b =>
        {
            b.OpenComponent<ToolbarSeparator>(0);
            b.CloseComponent();
        }));

        // Horizontal toolbar → vertical separator
        var sep = cut.Find("[role='separator']");
        Assert.Equal("vertical", sep.GetAttribute("aria-orientation"));
        Assert.Equal("vertical", sep.GetAttribute("data-orientation"));
    }

    [Fact]
    public void Separator_IsPerpendicularToVerticalToolbar()
    {
        var cut = Render(builder =>
        {
            builder.OpenComponent<ToolbarRoot>(0);
            builder.AddComponentParameter(1, "Orientation", Orientation.Vertical);
            builder.AddComponentParameter(2, "ChildContent", (RenderFragment)(b =>
            {
                b.OpenComponent<ToolbarSeparator>(0);
                b.CloseComponent();
            }));
            builder.CloseComponent();
        });

        // Vertical toolbar → horizontal separator
        Assert.Equal("horizontal", cut.Find("[role='separator']").GetAttribute("aria-orientation"));
    }

    // --- Button ---

    [Fact]
    public void Button_RendersAsButton()
    {
        var cut = RenderWithRoot((RenderFragment)(b =>
        {
            b.OpenComponent<ToolbarButton>(0);
            b.CloseComponent();
        }));

        Assert.Equal("BUTTON", cut.Find("button").TagName);
    }

    [Fact]
    public void Button_HasDataOrientationFromRoot()
    {
        var cut = RenderWithRoot((RenderFragment)(b =>
        {
            b.OpenComponent<ToolbarButton>(0);
            b.CloseComponent();
        }));

        Assert.Equal("horizontal", cut.Find("button").GetAttribute("data-orientation"));
    }

    [Fact]
    public void Button_Disabled_UsesAriaDisabledByDefault()
    {
        // FocusableWhenDisabled defaults to true, so aria-disabled is used rather than the native
        // disabled attribute, keeping the button reachable via keyboard navigation.
        var cut = RenderWithRoot((RenderFragment)(b =>
        {
            b.OpenComponent<ToolbarButton>(0);
            b.AddComponentParameter(1, "Disabled", true);
            b.CloseComponent();
        }));

        var btn = cut.Find("button");
        Assert.Equal("true", btn.GetAttribute("aria-disabled"));
        Assert.NotNull(btn.GetAttribute("data-disabled"));
        Assert.Null(btn.GetAttribute("disabled"));
    }

    [Fact]
    public void Button_Disabled_WithFocusableWhenDisabledFalse_UsesNativeDisabled()
    {
        var cut = RenderWithRoot((RenderFragment)(b =>
        {
            b.OpenComponent<ToolbarButton>(0);
            b.AddComponentParameter(1, "Disabled", true);
            b.AddComponentParameter(2, "FocusableWhenDisabled", false);
            b.CloseComponent();
        }));

        var btn = cut.Find("button");
        Assert.NotNull(btn.GetAttribute("disabled"));
        Assert.Null(btn.GetAttribute("aria-disabled"));
    }

    [Fact]
    public void Button_HasDataFocusableByDefault()
    {
        // data-focusable is present when FocusableWhenDisabled is true (the default).
        var cut = RenderWithRoot((RenderFragment)(b =>
        {
            b.OpenComponent<ToolbarButton>(0);
            b.CloseComponent();
        }));

        Assert.NotNull(cut.Find("button").GetAttribute("data-focusable"));
    }

    [Fact]
    public void Button_NotDisabled_OmitsAriaDisabledAndNativeDisabled()
    {
        var cut = RenderWithRoot((RenderFragment)(b =>
        {
            b.OpenComponent<ToolbarButton>(0);
            b.CloseComponent();
        }));

        var btn = cut.Find("button");
        Assert.Null(btn.GetAttribute("aria-disabled"));
        Assert.Null(btn.GetAttribute("disabled"));
    }

    // --- Root disabled cascades to children ---

    [Fact]
    public void Root_Disabled_CascadesToButton()
    {
        var cut = Render(builder =>
        {
            builder.OpenComponent<ToolbarRoot>(0);
            builder.AddComponentParameter(1, "Disabled", true);
            builder.AddComponentParameter(2, "ChildContent", (RenderFragment)(b =>
            {
                b.OpenComponent<ToolbarButton>(0);
                b.CloseComponent();
            }));
            builder.CloseComponent();
        });

        var btn = cut.Find("button");
        Assert.Equal("true", btn.GetAttribute("aria-disabled"));
        Assert.NotNull(btn.GetAttribute("data-disabled"));
    }

    [Fact]
    public void Root_Disabled_CascadesToInput()
    {
        var cut = Render(builder =>
        {
            builder.OpenComponent<ToolbarRoot>(0);
            builder.AddComponentParameter(1, "Disabled", true);
            builder.AddComponentParameter(2, "ChildContent", (RenderFragment)(b =>
            {
                b.OpenComponent<ToolbarInput>(0);
                b.CloseComponent();
            }));
            builder.CloseComponent();
        });

        Assert.Equal("true", cut.Find("input").GetAttribute("aria-disabled"));
    }

    [Fact]
    public void Root_Disabled_DoesNotDisableLink()
    {
        // Links cannot be disabled per the Base UI specification.
        var cut = Render(builder =>
        {
            builder.OpenComponent<ToolbarRoot>(0);
            builder.AddComponentParameter(1, "Disabled", true);
            builder.AddComponentParameter(2, "ChildContent", (RenderFragment)(b =>
            {
                b.OpenComponent<ToolbarLink>(0);
                b.CloseComponent();
            }));
            builder.CloseComponent();
        });

        var link = cut.Find("a");
        Assert.Null(link.GetAttribute("aria-disabled"));
        Assert.Null(link.GetAttribute("data-disabled"));
    }

    // --- Group ---

    [Fact]
    public void Group_HasGroupRole()
    {
        var cut = RenderWithRoot((RenderFragment)(b =>
        {
            b.OpenComponent<ToolbarGroup>(0);
            b.CloseComponent();
        }));

        Assert.NotNull(cut.Find("[role='group']"));
    }

    [Fact]
    public void Group_HasDataOrientation()
    {
        var cut = RenderWithRoot((RenderFragment)(b =>
        {
            b.OpenComponent<ToolbarGroup>(0);
            b.CloseComponent();
        }));

        Assert.Equal("horizontal", cut.Find("[role='group']").GetAttribute("data-orientation"));
    }

    [Fact]
    public void Group_Disabled_SetsDataDisabledOnGroup()
    {
        var cut = RenderWithRoot((RenderFragment)(b =>
        {
            b.OpenComponent<ToolbarGroup>(0);
            b.AddComponentParameter(1, "Disabled", true);
            b.CloseComponent();
        }));

        Assert.NotNull(cut.Find("[role='group']").GetAttribute("data-disabled"));
    }

    [Fact]
    public void Group_Disabled_CascadesToButtonsAndInputs()
    {
        var cut = RenderWithRoot((RenderFragment)(b =>
        {
            b.OpenComponent<ToolbarGroup>(0);
            b.AddComponentParameter(1, "Disabled", true);
            b.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<ToolbarButton>(0);
                inner.CloseComponent();
                inner.OpenComponent<ToolbarInput>(1);
                inner.CloseComponent();
            }));
            b.CloseComponent();
        }));

        Assert.Equal("true", cut.Find("button").GetAttribute("aria-disabled"));
        Assert.NotNull(cut.Find("button").GetAttribute("data-disabled"));
        Assert.Equal("true", cut.Find("input").GetAttribute("aria-disabled"));
        Assert.NotNull(cut.Find("input").GetAttribute("data-disabled"));
    }

    [Fact]
    public void Group_Disabled_DoesNotDisableLinkInsideGroup()
    {
        var cut = RenderWithRoot((RenderFragment)(b =>
        {
            b.OpenComponent<ToolbarGroup>(0);
            b.AddComponentParameter(1, "Disabled", true);
            b.AddComponentParameter(2, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<ToolbarLink>(0);
                inner.CloseComponent();
            }));
            b.CloseComponent();
        }));

        var link = cut.Find("a");
        Assert.Null(link.GetAttribute("aria-disabled"));
        Assert.Null(link.GetAttribute("data-disabled"));
    }

    // --- Input ---

    [Fact]
    public void Input_RendersAsInput()
    {
        var cut = RenderWithRoot((RenderFragment)(b =>
        {
            b.OpenComponent<ToolbarInput>(0);
            b.CloseComponent();
        }));

        Assert.Equal("INPUT", cut.Find("input").TagName);
    }

    [Fact]
    public void Input_HasDataOrientation()
    {
        var cut = RenderWithRoot((RenderFragment)(b =>
        {
            b.OpenComponent<ToolbarInput>(0);
            b.CloseComponent();
        }));

        Assert.Equal("horizontal", cut.Find("input").GetAttribute("data-orientation"));
    }

    // --- Link ---

    [Fact]
    public void Link_RendersAsAnchor()
    {
        var cut = RenderWithRoot((RenderFragment)(b =>
        {
            b.OpenComponent<ToolbarLink>(0);
            b.CloseComponent();
        }));

        Assert.Equal("A", cut.Find("a").TagName);
    }

    [Fact]
    public void Link_HasDataOrientation()
    {
        var cut = RenderWithRoot((RenderFragment)(b =>
        {
            b.OpenComponent<ToolbarLink>(0);
            b.CloseComponent();
        }));

        Assert.Equal("horizontal", cut.Find("a").GetAttribute("data-orientation"));
    }

    // --- Helpers ---

    /// <summary>
    /// Renders child content inside a default horizontal <see cref="ToolbarRoot"/>.
    /// </summary>
    private Bunit.IRenderedComponent<Bunit.Rendering.ContainerFragment> RenderWithRoot(RenderFragment childContent)
    {
        return Render(builder =>
        {
            builder.OpenComponent<ToolbarRoot>(0);
            builder.AddComponentParameter(1, "ChildContent", childContent);
            builder.CloseComponent();
        });
    }
}
