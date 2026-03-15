using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.ContextMenu;

/// <summary>
/// The context area — right-clicking anywhere inside this element opens the context menu.
/// Unlike MenuTrigger, this captures <c>contextmenu</c> not click.
/// </summary>
/// <remarks>
/// Keeps its own <see cref="BuildRenderTree"/> because <c>AddEventPreventDefaultAttribute</c>
/// is a builder-only API that cannot be represented in an attribute dictionary.
/// </remarks>
public class ContextMenuTrigger : BlazeElement<ContextMenuTriggerState>
{
    [CascadingParameter] internal ContextMenuContext Context { get; set; } = default!;

    // We need a reference to the root to call OpenAtPositionAsync.
    [CascadingParameter] internal ContextMenuRoot? Root { get; set; }

    /// <summary>Whether the component should ignore user interaction.</summary>
    [Parameter] public bool Disabled { get; set; }

    // Tracks whether a pointer press is currently active for data-pressed.
    private bool _pressed;

    protected override string DefaultTag => "div";
    protected override ContextMenuTriggerState GetCurrentState() => new(Context.Open, Disabled, _pressed);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-popup-open", Context.Open ? "" : null);
        yield return new("data-pressed", _pressed ? "" : null);
    }

    private bool IsDisabled => Disabled || Context.Disabled;

    private async Task HandleContextMenu(MouseEventArgs e)
    {
        if (IsDisabled) return;
        Context.CursorX = e.ClientX;
        Context.CursorY = e.ClientY;
        await Context.SetOpen(true);
        // Release pressed state — the pointer is no longer held after the context menu fires.
        _pressed = false;
    }

    private void HandlePointerDown(PointerEventArgs e)
    {
        if (IsDisabled) return;
        _pressed = true;
        StateHasChanged();
    }

    private void HandlePointerUp(PointerEventArgs e)
    {
        if (!_pressed) return;
        _pressed = false;
        StateHasChanged();
    }

    private void HandlePointerLeave(PointerEventArgs e)
    {
        if (!_pressed) return;
        _pressed = false;
        StateHasChanged();
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var attrs = BuildAttributes(state);

        var tag = As ?? DefaultTag;
        builder.OpenElement(0, tag);
        builder.AddMultipleAttributes(1, attrs);

        // Prevent the browser's native context menu only when enabled. Disabled triggers
        // restore native browser behaviour.
        builder.AddEventPreventDefaultAttribute(2, "oncontextmenu", !IsDisabled);

        builder.AddAttribute(3, "oncontextmenu",
            EventCallback.Factory.Create<MouseEventArgs>(this, HandleContextMenu));
        builder.AddAttribute(4, "onpointerdown",
            EventCallback.Factory.Create<PointerEventArgs>(this, HandlePointerDown));
        builder.AddAttribute(5, "onpointerup",
            EventCallback.Factory.Create<PointerEventArgs>(this, HandlePointerUp));
        builder.AddAttribute(6, "onpointerleave",
            EventCallback.Factory.Create<PointerEventArgs>(this, HandlePointerLeave));

        builder.AddContent(7, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct ContextMenuTriggerState(bool Open, bool Disabled, bool Pressed);
