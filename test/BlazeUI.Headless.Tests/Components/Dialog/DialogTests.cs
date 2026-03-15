using BlazeUI.Headless.Components.Dialog;
using BlazeUI.Headless.Core;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Headless.Tests.Components.Dialog;

public class DialogTests : BunitContext
{
    public DialogTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazeUI();
    }

    // Creates a context for unit testing sub-parts in isolation (no Root required).
    private DialogContext CreateContext(bool open = false, string popupId = "test-dialog-popup") => new()
    {
        Open = open,
        PopupId = popupId,
        SetOpen = _ => Task.CompletedTask,
        Close = () => Task.CompletedTask,
    };

    // Renders a minimal but complete dialog tree: Root → Trigger + Popup.
    // Portal is omitted so the popup renders inline — sufficient for unit tests.
    private IRenderedComponent<DialogRoot> RenderDialog(
        bool defaultOpen = false,
        Action<ComponentParameterCollectionBuilder<DialogRoot>>? configure = null)
    {
        return Render<DialogRoot>(builder =>
        {
            builder.Add(p => p.DefaultOpen, defaultOpen);

            configure?.Invoke(builder);

            builder.Add(p => p.ChildContent, (RenderFragment)(b =>
            {
                b.OpenComponent<DialogTrigger>(0);
                b.AddComponentParameter(1, "ChildContent", (RenderFragment)(inner =>
                    inner.AddContent(0, "Open")));
                b.CloseComponent();

                b.OpenComponent<DialogPopup>(2);
                b.AddComponentParameter(3, "ChildContent", (RenderFragment)(inner =>
                {
                    inner.OpenComponent<DialogTitle>(0);
                    inner.AddComponentParameter(1, "ChildContent", (RenderFragment)(t =>
                        t.AddContent(0, "Dialog title")));
                    inner.CloseComponent();

                    inner.OpenComponent<DialogDescription>(2);
                    inner.AddComponentParameter(3, "ChildContent", (RenderFragment)(t =>
                        t.AddContent(0, "Dialog description")));
                    inner.CloseComponent();

                    inner.OpenComponent<DialogClose>(4);
                    inner.AddComponentParameter(5, "ChildContent", (RenderFragment)(t =>
                        t.AddContent(0, "Close")));
                    inner.CloseComponent();
                }));
                b.CloseComponent();
            }));
        });
    }

    // ── Integration tests (full Root tree) ──────────────────────────────────

    [Fact]
    public void Trigger_HasCorrectAriaAttributes_WhenClosed()
    {
        var cut = RenderDialog();

        var trigger = cut.Find("button");
        Assert.Equal("dialog", trigger.GetAttribute("aria-haspopup"));
        Assert.Equal("false", trigger.GetAttribute("aria-expanded"));
        Assert.NotNull(trigger.GetAttribute("aria-controls"));
    }

    [Fact]
    public void Trigger_HasAriaExpandedTrue_WhenOpen()
    {
        var cut = RenderDialog(defaultOpen: true);

        // The first button is the trigger; the second is the Close button inside the popup.
        var trigger = cut.FindAll("button")[0];
        Assert.Equal("true", trigger.GetAttribute("aria-expanded"));
    }

    [Fact]
    public void Trigger_AriaControls_MatchesPopupId()
    {
        var cut = RenderDialog(defaultOpen: true);

        var trigger = cut.FindAll("button")[0];
        var popup = cut.Find("[role=dialog]");

        Assert.Equal(popup.GetAttribute("id"), trigger.GetAttribute("aria-controls"));
    }

    [Fact]
    public void Popup_HasAriaModal()
    {
        var cut = RenderDialog(defaultOpen: true);

        var popup = cut.Find("[role=dialog]");
        Assert.Equal("true", popup.GetAttribute("aria-modal"));
    }

    [Fact]
    public void Popup_HasTabIndexMinusOne()
    {
        // tabindex="-1" ensures the popup can receive programmatic focus even when
        // it contains no tabbable descendants.
        var cut = RenderDialog(defaultOpen: true);

        var popup = cut.Find("[role=dialog]");
        Assert.Equal("-1", popup.GetAttribute("tabindex"));
    }

    [Fact]
    public void Popup_HasAriaLabelledBy_PointingToTitle()
    {
        var cut = RenderDialog(defaultOpen: true);

        var popup = cut.Find("[role=dialog]");
        var title = cut.Find("h2");

        Assert.Equal(title.GetAttribute("id"), popup.GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void Popup_HasAriaDescribedBy_PointingToDescription()
    {
        var cut = RenderDialog(defaultOpen: true);

        var popup = cut.Find("[role=dialog]");
        var description = cut.Find("p");

        Assert.Equal(description.GetAttribute("id"), popup.GetAttribute("aria-describedby"));
    }

    [Fact]
    public void Popup_NotMounted_WhenClosed()
    {
        var cut = RenderDialog();

        // The popup element is not in the DOM when the dialog is closed.
        Assert.Empty(cut.FindAll("[role=dialog]"));
    }

    [Fact]
    public void Trigger_Click_OpensDialog()
    {
        var cut = RenderDialog();

        cut.Find("button").Click();

        Assert.NotEmpty(cut.FindAll("[role=dialog]"));
        Assert.Equal("true", cut.FindAll("button")[0].GetAttribute("aria-expanded"));
    }

    [Fact]
    public void Close_Click_CollapsesAriaExpanded()
    {
        // Clicking the Close button should set open=false, collapsing aria-expanded on the
        // trigger. The popup element remains in the DOM until the JS exit animation completes,
        // so we verify state via the trigger's aria-expanded rather than popup presence.
        var cut = RenderDialog(defaultOpen: true);

        Assert.Equal("true", cut.FindAll("button")[0].GetAttribute("aria-expanded"));

        cut.FindAll("button").Last().Click();

        Assert.Equal("false", cut.FindAll("button")[0].GetAttribute("aria-expanded"));
    }

    // data-open/data-closed are owned by JS (not rendered by Blazor) to
    // avoid re-render cycles resetting animation state. Verified by E2E tests.

    [Fact]
    public void OpenChanged_Fires_WhenTriggerClicked()
    {
        bool? received = null;

        var cut = RenderDialog(configure: b =>
            b.Add(p => p.OpenChanged, EventCallback.Factory.Create<bool>(this, v => received = v)));

        cut.Find("button").Click();

        Assert.True(received);
    }

    [Fact]
    public void OpenChanged_Fires_False_WhenClosedViaCloseButton()
    {
        bool? received = null;

        var cut = RenderDialog(
            defaultOpen: true,
            configure: b =>
                b.Add(p => p.OpenChanged, EventCallback.Factory.Create<bool>(this, v => received = v)));

        cut.FindAll("button").Last().Click();

        Assert.False(received);
    }

    [Fact]
    public void Backdrop_HasPresentationRole()
    {
        var cut = Render<DialogRoot>(builder =>
        {
            builder.Add(p => p.DefaultOpen, true);
            builder.Add(p => p.ChildContent, (RenderFragment)(b =>
            {
                b.OpenComponent<DialogBackdrop>(0);
                b.CloseComponent();

                b.OpenComponent<DialogPopup>(2);
                b.AddComponentParameter(3, "ChildContent", (RenderFragment)(c =>
                    c.AddContent(0, "content")));
                b.CloseComponent();
            }));
        });

        var backdrop = cut.Find("[data-open]:not([role=dialog])");
        Assert.Equal("presentation", backdrop.GetAttribute("role"));
    }

    [Fact]
    public void Backdrop_HasNoClickHandler()
    {
        // Regular (non-AlertDialog) dialogs do not prevent backdrop close; however, the
        // backdrop itself carries no onclick — dismissal is handled at the JS level via
        // pointer-outside detection, not via Blazor event handlers on the backdrop element.
        var cut = Render<DialogRoot>(builder =>
        {
            builder.Add(p => p.DefaultOpen, true);
            builder.Add(p => p.ChildContent, (RenderFragment)(b =>
            {
                b.OpenComponent<DialogBackdrop>(0);
                b.CloseComponent();

                b.OpenComponent<DialogPopup>(2);
                b.AddComponentParameter(3, "ChildContent", (RenderFragment)(c =>
                    c.AddContent(0, "content")));
                b.CloseComponent();
            }));
        });

        var backdrop = cut.Find("[data-open]:not([role=dialog])");
        Assert.False(backdrop.HasAttribute("onclick"));
    }

    // ── Sub-part unit tests (context injection) ──────────────────────────────

    [Fact]
    public void DialogTrigger_HasAriaExpandedFalseWhenClosed()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<DialogContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DialogTrigger>());

        Assert.Equal("false", cut.Find("button").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void DialogTrigger_HasAriaExpandedTrueWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<DialogContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DialogTrigger>());

        Assert.Equal("true", cut.Find("button").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void DialogTrigger_HasAriaControlsPointingToPopup()
    {
        var ctx = CreateContext(open: false, popupId: "my-dialog");
        var cut = Render<CascadingValue<DialogContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DialogTrigger>());

        Assert.Equal("my-dialog", cut.Find("button").GetAttribute("aria-controls"));
    }

    [Fact]
    public void DialogTrigger_HasAriaHaspopupDialog()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<DialogContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DialogTrigger>());

        Assert.Equal("dialog", cut.Find("button").GetAttribute("aria-haspopup"));
    }

    [Fact]
    public void DialogTrigger_HasDataPopupOpenWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<DialogContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DialogTrigger>());

        Assert.NotNull(cut.Find("[data-popup-open]"));
    }

    [Fact]
    public void DialogTrigger_NoDataPopupOpenWhenClosed()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<DialogContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DialogTrigger>());

        Assert.Empty(cut.FindAll("[data-popup-open]"));
    }

    [Fact]
    public void DialogTrigger_Disabled_HasDataDisabledAndDisabledAttr()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<DialogContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DialogTrigger>(tp => tp.Add(c => c.Disabled, true)));

        var button = cut.Find("button");
        Assert.NotNull(cut.Find("[data-disabled]"));
        Assert.True(button.HasAttribute("disabled"));
    }

    [Fact]
    public void DialogTrigger_Disabled_DoesNotCallSetOpen()
    {
        // Arrange: a context whose SetOpen records invocations.
        bool wasCalledOpen = false;
        var ctx = CreateContext(open: false);
        ctx.SetOpen = v => { wasCalledOpen = true; return Task.CompletedTask; };

        var cut = Render<CascadingValue<DialogContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DialogTrigger>(tp => tp.Add(c => c.Disabled, true)));

        // Act
        cut.Find("button").Click();

        // Assert
        Assert.False(wasCalledOpen);
    }

    [Fact]
    public void DialogClose_RendersButton()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<DialogContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DialogClose>(cp => cp.AddChildContent("Close")));

        Assert.NotNull(cut.Find("button"));
    }

    [Fact]
    public void DialogClose_Disabled_HasDataDisabledAndDisabledAttr()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<DialogContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DialogClose>(cp => cp
                .Add(c => c.Disabled, true)
                .AddChildContent("Close")));

        var button = cut.Find("button");
        Assert.NotNull(cut.Find("[data-disabled]"));
        Assert.True(button.HasAttribute("disabled"));
    }

    [Fact]
    public void DialogClose_Disabled_DoesNotCallClose()
    {
        bool wasCalled = false;
        var ctx = CreateContext(open: true);
        ctx.Close = () => { wasCalled = true; return Task.CompletedTask; };

        var cut = Render<CascadingValue<DialogContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DialogClose>(cp => cp
                .Add(c => c.Disabled, true)
                .AddChildContent("Close")));

        // Act
        cut.Find("button").Click();

        // Assert
        Assert.False(wasCalled);
    }

    [Fact]
    public void DialogClose_NotDisabled_NoDataDisabled()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<DialogContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DialogClose>(cp => cp.AddChildContent("Close")));

        Assert.Empty(cut.FindAll("[data-disabled]"));
    }

    [Fact]
    public void DialogBackdrop_HasPresentationRole()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<DialogContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DialogBackdrop>());

        Assert.Equal("presentation", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void DialogBackdrop_HasDataOpenWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<DialogContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DialogBackdrop>());

        Assert.NotNull(cut.Find("[data-open]"));
        Assert.Empty(cut.FindAll("[data-closed]"));
    }

    [Fact]
    public void DialogBackdrop_NotMounted_WhenNeverOpened()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<DialogContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DialogBackdrop>());

        // Backdrop should not render until the dialog has been opened at least once.
        Assert.Empty(cut.FindAll("[role=presentation]"));
    }

    [Fact]
    public void DialogBackdrop_RendersWhenNotNested()
    {
        // A single (non-nested) dialog's backdrop should always render.
        var cut = Render<DialogRoot>(builder =>
        {
            builder.Add(p => p.DefaultOpen, true);
            builder.Add(p => p.ChildContent, (RenderFragment)(b =>
            {
                b.OpenComponent<DialogBackdrop>(0);
                b.CloseComponent();

                b.OpenComponent<DialogPopup>(2);
                b.AddComponentParameter(3, "ChildContent", (RenderFragment)(c =>
                    c.AddContent(0, "content")));
                b.CloseComponent();
            }));
        });

        Assert.NotEmpty(cut.FindAll("[role=presentation]"));
    }

    [Fact]
    public void DialogBackdrop_SuppressedWhenNested()
    {
        // Only the outermost dialog's backdrop renders; nested backdrops
        // are suppressed by default (matching Base UI behavior).
        var cut = Render<DialogRoot>(builder =>
        {
            builder.Add(p => p.DefaultOpen, true);
            builder.Add(p => p.ChildContent, (RenderFragment)(b =>
            {
                b.OpenComponent<DialogBackdrop>(0);
                b.AddComponentParameter(1, "data-testid", "root-backdrop");
                b.CloseComponent();

                b.OpenComponent<DialogPopup>(2);
                b.AddComponentParameter(3, "ChildContent", (RenderFragment)(inner =>
                {
                    // Nested dialog inside the popup content.
                    inner.OpenComponent<DialogRoot>(0);
                    inner.AddComponentParameter(1, "DefaultOpen", true);
                    inner.AddComponentParameter(2, "ChildContent", (RenderFragment)(nested =>
                    {
                        nested.OpenComponent<DialogBackdrop>(0);
                        nested.AddComponentParameter(1, "data-testid", "nested-backdrop");
                        nested.CloseComponent();

                        nested.OpenComponent<DialogPopup>(2);
                        nested.AddComponentParameter(3, "ChildContent", (RenderFragment)(c =>
                            c.AddContent(0, "Nested dialog")));
                        nested.CloseComponent();
                    }));
                    inner.CloseComponent();
                }));
                b.CloseComponent();
            }));
        });

        // Root backdrop renders.
        Assert.NotNull(cut.Find("[data-testid=root-backdrop]"));
        // Nested backdrop is suppressed.
        Assert.Empty(cut.FindAll("[data-testid=nested-backdrop]"));
    }

    [Fact]
    public void DialogBackdrop_SuppressedAtMultipleNestingLevels()
    {
        // Three levels of nesting — only the outermost backdrop renders.
        var cut = Render<DialogRoot>(builder =>
        {
            builder.Add(p => p.DefaultOpen, true);
            builder.Add(p => p.ChildContent, (RenderFragment)(b =>
            {
                b.OpenComponent<DialogBackdrop>(0);
                b.AddComponentParameter(1, "data-testid", "level-1-backdrop");
                b.CloseComponent();

                b.OpenComponent<DialogPopup>(2);
                b.AddComponentParameter(3, "ChildContent", (RenderFragment)(inner =>
                {
                    inner.OpenComponent<DialogRoot>(0);
                    inner.AddComponentParameter(1, "DefaultOpen", true);
                    inner.AddComponentParameter(2, "ChildContent", (RenderFragment)(l2 =>
                    {
                        l2.OpenComponent<DialogBackdrop>(0);
                        l2.AddComponentParameter(1, "data-testid", "level-2-backdrop");
                        l2.CloseComponent();

                        l2.OpenComponent<DialogPopup>(2);
                        l2.AddComponentParameter(3, "ChildContent", (RenderFragment)(l2inner =>
                        {
                            l2inner.OpenComponent<DialogRoot>(0);
                            l2inner.AddComponentParameter(1, "DefaultOpen", true);
                            l2inner.AddComponentParameter(2, "ChildContent", (RenderFragment)(l3 =>
                            {
                                l3.OpenComponent<DialogBackdrop>(0);
                                l3.AddComponentParameter(1, "data-testid", "level-3-backdrop");
                                l3.CloseComponent();

                                l3.OpenComponent<DialogPopup>(2);
                                l3.AddComponentParameter(3, "ChildContent", (RenderFragment)(c =>
                                    c.AddContent(0, "Level 3 dialog")));
                                l3.CloseComponent();
                            }));
                            l2inner.CloseComponent();
                        }));
                        l2.CloseComponent();
                    }));
                    inner.CloseComponent();
                }));
                b.CloseComponent();
            }));
        });

        Assert.NotNull(cut.Find("[data-testid=level-1-backdrop]"));
        Assert.Empty(cut.FindAll("[data-testid=level-2-backdrop]"));
        Assert.Empty(cut.FindAll("[data-testid=level-3-backdrop]"));
    }

    [Fact]
    public void DialogBackdrop_ForceRender_RendersAllNested()
    {
        // With ForceRender=true, all nested backdrops render.
        var cut = Render<DialogRoot>(builder =>
        {
            builder.Add(p => p.DefaultOpen, true);
            builder.Add(p => p.ChildContent, (RenderFragment)(b =>
            {
                b.OpenComponent<DialogBackdrop>(0);
                b.AddComponentParameter(1, "data-testid", "level-1-backdrop");
                b.AddComponentParameter(2, nameof(DialogBackdrop.ForceRender), true);
                b.CloseComponent();

                b.OpenComponent<DialogPopup>(2);
                b.AddComponentParameter(3, "ChildContent", (RenderFragment)(inner =>
                {
                    inner.OpenComponent<DialogRoot>(0);
                    inner.AddComponentParameter(1, "DefaultOpen", true);
                    inner.AddComponentParameter(2, "ChildContent", (RenderFragment)(l2 =>
                    {
                        l2.OpenComponent<DialogBackdrop>(0);
                        l2.AddComponentParameter(1, "data-testid", "level-2-backdrop");
                        l2.AddComponentParameter(2, nameof(DialogBackdrop.ForceRender), true);
                        l2.CloseComponent();

                        l2.OpenComponent<DialogPopup>(2);
                        l2.AddComponentParameter(3, "ChildContent", (RenderFragment)(l2inner =>
                        {
                            l2inner.OpenComponent<DialogRoot>(0);
                            l2inner.AddComponentParameter(1, "DefaultOpen", true);
                            l2inner.AddComponentParameter(2, "ChildContent", (RenderFragment)(l3 =>
                            {
                                l3.OpenComponent<DialogBackdrop>(0);
                                l3.AddComponentParameter(1, "data-testid", "level-3-backdrop");
                                l3.AddComponentParameter(2, nameof(DialogBackdrop.ForceRender), true);
                                l3.CloseComponent();

                                l3.OpenComponent<DialogPopup>(2);
                                l3.AddComponentParameter(3, "ChildContent", (RenderFragment)(c =>
                                    c.AddContent(0, "Level 3 dialog")));
                                l3.CloseComponent();
                            }));
                            l2inner.CloseComponent();
                        }));
                        l2.CloseComponent();
                    }));
                    inner.CloseComponent();
                }));
                b.CloseComponent();
            }));
        });

        Assert.NotNull(cut.Find("[data-testid=level-1-backdrop]"));
        Assert.NotNull(cut.Find("[data-testid=level-2-backdrop]"));
        Assert.NotNull(cut.Find("[data-testid=level-3-backdrop]"));
    }

    [Fact]
    public void DialogTitle_RendersH2ByDefault()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<DialogContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DialogTitle>(tp => tp.AddChildContent("My Title")));

        Assert.NotNull(cut.Find("h2"));
        Assert.Contains("My Title", cut.Markup);
    }

    [Fact]
    public void DialogTitle_SetsContextTitleId()
    {
        var ctx = CreateContext();
        Render<CascadingValue<DialogContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DialogTitle>(tp => tp.AddChildContent("Title")));

        Assert.False(string.IsNullOrEmpty(ctx.TitleId));
    }

    [Fact]
    public void DialogTitle_IdMatchesContextTitleId()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<DialogContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DialogTitle>(tp => tp.AddChildContent("Title")));

        var h2Id = cut.Find("h2").GetAttribute("id");
        Assert.Equal(ctx.TitleId, h2Id);
    }

    [Fact]
    public void DialogDescription_RendersParagraphByDefault()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<DialogContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DialogDescription>(dp => dp.AddChildContent("Some description")));

        Assert.NotNull(cut.Find("p"));
        Assert.Contains("Some description", cut.Markup);
    }

    [Fact]
    public void DialogDescription_SetsContextDescriptionId()
    {
        var ctx = CreateContext();
        Render<CascadingValue<DialogContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DialogDescription>(dp => dp.AddChildContent("Desc")));

        Assert.False(string.IsNullOrEmpty(ctx.DescriptionId));
    }

    [Fact]
    public void DialogDescription_IdMatchesContextDescriptionId()
    {
        var ctx = CreateContext();
        var cut = Render<CascadingValue<DialogContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DialogDescription>(dp => dp.AddChildContent("Desc")));

        var pId = cut.Find("p").GetAttribute("id");
        Assert.Equal(ctx.DescriptionId, pId);
    }

    [Fact]
    public void DialogViewport_HasPresentationRole()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<DialogContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DialogViewport>(vp => vp.AddChildContent("content")));

        Assert.Equal("presentation", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void DialogViewport_HasDataOpenWhenOpen()
    {
        var ctx = CreateContext(open: true);
        var cut = Render<CascadingValue<DialogContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DialogViewport>(vp => vp.AddChildContent("content")));

        Assert.NotNull(cut.Find("[data-open]"));
        Assert.Empty(cut.FindAll("[data-closed]"));
    }

    [Fact]
    public void DialogViewport_HasDataClosedWhenClosed()
    {
        var ctx = CreateContext(open: false);
        var cut = Render<CascadingValue<DialogContext>>(p => p
            .Add(c => c.Value, ctx)
            .AddChildContent<DialogViewport>(vp => vp.AddChildContent("content")));

        Assert.NotNull(cut.Find("[data-closed]"));
        Assert.Empty(cut.FindAll("[data-open]"));
    }
}
