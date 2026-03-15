using BlazeUI.Headless.Components.AlertDialog;
using BlazeUI.Headless.Components.Dialog;
using BlazeUI.Headless.Core;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Headless.Tests.Components.AlertDialog;

public class AlertDialogTests : BunitContext
{
    public AlertDialogTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazeUI();
    }

    // Renders a minimal but complete alert dialog tree: Root → Portal → Trigger + Popup.
    // The Portal is omitted so the popup renders inline — sufficient for unit tests.
    private IRenderedComponent<AlertDialogRoot> RenderAlertDialog(
        bool defaultOpen = false,
        Action<ComponentParameterCollectionBuilder<AlertDialogRoot>>? configure = null)
    {
        return Render<AlertDialogRoot>(builder =>
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
                        t.AddContent(0, "Alert title")));
                    inner.CloseComponent();

                    inner.OpenComponent<DialogDescription>(2);
                    inner.AddComponentParameter(3, "ChildContent", (RenderFragment)(t =>
                        t.AddContent(0, "Alert description")));
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

    [Fact]
    public void Trigger_HasCorrectAriaAttributes_WhenClosed()
    {
        var cut = RenderAlertDialog();

        var trigger = cut.Find("button");
        Assert.Equal("dialog", trigger.GetAttribute("aria-haspopup"));
        Assert.Equal("false", trigger.GetAttribute("aria-expanded"));
        // aria-controls must point at the popup element
        Assert.NotNull(trigger.GetAttribute("aria-controls"));
    }

    [Fact]
    public void Trigger_HasAriaExpandedTrue_WhenOpen()
    {
        var cut = RenderAlertDialog(defaultOpen: true);

        // The first button is the trigger; the second is the Close button inside the popup.
        var trigger = cut.FindAll("button")[0];
        Assert.Equal("true", trigger.GetAttribute("aria-expanded"));
    }

    [Fact]
    public void Trigger_AriaControls_MatchesPopupId()
    {
        var cut = RenderAlertDialog(defaultOpen: true);

        var trigger = cut.FindAll("button")[0];
        var popup = cut.Find("[role='alertdialog']");

        Assert.Equal(popup.GetAttribute("id"), trigger.GetAttribute("aria-controls"));
    }

    [Fact]
    public void Popup_HasAlertdialogRole()
    {
        var cut = RenderAlertDialog(defaultOpen: true);

        var popup = cut.Find("[role=alertdialog]");
        Assert.Equal("alertdialog", popup.GetAttribute("role"));
    }

    [Fact]
    public void Popup_HasAriaModal()
    {
        var cut = RenderAlertDialog(defaultOpen: true);

        var popup = cut.Find("[role=alertdialog]");
        Assert.Equal("true", popup.GetAttribute("aria-modal"));
    }

    [Fact]
    public void Popup_HasAriaLabelledBy_PointingToTitle()
    {
        var cut = RenderAlertDialog(defaultOpen: true);

        var popup = cut.Find("[role=alertdialog]");
        var title = cut.Find("h2");

        Assert.Equal(title.GetAttribute("id"), popup.GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void Popup_HasAriaDescribedBy_PointingToDescription()
    {
        var cut = RenderAlertDialog(defaultOpen: true);

        var popup = cut.Find("[role=alertdialog]");
        var description = cut.Find("p");

        Assert.Equal(description.GetAttribute("id"), popup.GetAttribute("aria-describedby"));
    }

    [Fact]
    public void Popup_NotMounted_WhenClosed()
    {
        var cut = RenderAlertDialog();

        // The popup element is not in the DOM when the dialog is closed.
        Assert.Empty(cut.FindAll("[role=alertdialog]"));
    }

    [Fact]
    public void Trigger_Click_OpensDialog()
    {
        var cut = RenderAlertDialog();

        cut.Find("button").Click();

        Assert.NotEmpty(cut.FindAll("[role=alertdialog]"));
        Assert.Equal("true", cut.FindAll("button")[0].GetAttribute("aria-expanded"));
    }

    [Fact]
    public void Close_Click_CollapsesAriaExpanded()
    {
        // Clicking the Close button should set open=false, collapsing aria-expanded on the trigger.
        // The popup element remains in DOM until JS animation completes, so we check state via the
        // trigger's aria-expanded rather than presence of the dialog element.
        var cut = RenderAlertDialog(defaultOpen: true);

        Assert.Equal("true", cut.FindAll("button")[0].GetAttribute("aria-expanded"));

        // Re-query after the last render to avoid stale element references.
        cut.FindAll("button").Last().Click();

        Assert.Equal("false", cut.FindAll("button")[0].GetAttribute("aria-expanded"));
    }

    // data-open/data-closed are owned by JS (not rendered by Blazor) to
    // avoid re-render cycles resetting animation state. Verified by E2E tests.

    [Fact]
    public void OpenChanged_Fires_WhenTriggerClicked()
    {
        bool? received = null;

        var cut = RenderAlertDialog(configure: b =>
            b.Add(p => p.OpenChanged, EventCallback.Factory.Create<bool>(this, v => received = v)));

        cut.Find("button").Click();

        Assert.True(received);
    }

    [Fact]
    public void Backdrop_HasNoClickHandler()
    {
        // Alert dialogs must not close when the backdrop is clicked.
        // Verify by confirming the backdrop element carries no onclick attribute.
        var cut = Render<AlertDialogRoot>(builder =>
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

        // The backdrop has data-open (it is rendered) but no onclick handler.
        var backdrop = cut.Find("[data-open]:not([role=alertdialog])");
        Assert.False(backdrop.HasAttribute("onclick"));
    }
}
