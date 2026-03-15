using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Select;

/// <summary>
/// Hover-activated scroll indicator at the top of the select popup. Automatically
/// scrolls the list upward when hovered. Hidden when the list is not scrollable
/// in the up direction.
/// </summary>
public class SelectScrollUpArrow : BlazeElement<SelectScrollArrowState>
{
    [CascadingParameter] internal SelectContext Context { get; set; } = default!;

    /// <summary>
    /// When <c>true</c>, the arrow remains in the DOM even when not visible,
    /// enabling CSS transition animations.
    /// </summary>
    [Parameter] public bool KeepMounted { get; set; }

    private bool _visible;
    private int? _scrollTimerId;

    protected override string DefaultTag => "div";

    protected override SelectScrollArrowState GetCurrentState() => new(_visible);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-direction", "up");
        yield return new("data-visible", _visible ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("aria-hidden", "true");
        yield return new("onmouseenter",
            EventCallback.Factory.Create<MouseEventArgs>(this, OnMouseEnter));
        yield return new("onmouseleave",
            EventCallback.Factory.Create<MouseEventArgs>(this, OnMouseLeave));
    }

    private async Task OnMouseEnter()
    {
        if (Context.JsModule is null) return;
        try
        {
            _scrollTimerId = await Context.JsModule.InvokeAsync<int>(
                "startScrollArrow", Context.PopupId, "up");
        }
        catch (Microsoft.JSInterop.JSDisconnectedException) { }
        catch (OperationCanceledException) { }
    }

    private async Task OnMouseLeave()
    {
        if (Context.JsModule is null || _scrollTimerId is null) return;
        try
        {
            await Context.JsModule.InvokeVoidAsync("stopScrollArrow", _scrollTimerId);
            _scrollTimerId = null;
        }
        catch (Microsoft.JSInterop.JSDisconnectedException) { }
        catch (OperationCanceledException) { }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Context.Open && Context.JsModule is not null)
        {
            try
            {
                _visible = await Context.JsModule.InvokeAsync<bool>(
                    "isScrollable", Context.PopupId, "up");
                StateHasChanged();
            }
            catch (Microsoft.JSInterop.JSDisconnectedException) { }
            catch (OperationCanceledException) { }
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!_visible && !KeepMounted) return;
        base.BuildRenderTree(builder);
    }
}

/// <summary>
/// Hover-activated scroll indicator at the bottom of the select popup. Automatically
/// scrolls the list downward when hovered. Hidden when the list is not scrollable
/// in the down direction.
/// </summary>
public class SelectScrollDownArrow : BlazeElement<SelectScrollArrowState>
{
    [CascadingParameter] internal SelectContext Context { get; set; } = default!;

    /// <summary>
    /// When <c>true</c>, the arrow remains in the DOM even when not visible,
    /// enabling CSS transition animations.
    /// </summary>
    [Parameter] public bool KeepMounted { get; set; }

    private bool _visible;
    private int? _scrollTimerId;

    protected override string DefaultTag => "div";

    protected override SelectScrollArrowState GetCurrentState() => new(_visible);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-direction", "down");
        yield return new("data-visible", _visible ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("aria-hidden", "true");
        yield return new("onmouseenter",
            EventCallback.Factory.Create<MouseEventArgs>(this, OnMouseEnter));
        yield return new("onmouseleave",
            EventCallback.Factory.Create<MouseEventArgs>(this, OnMouseLeave));
    }

    private async Task OnMouseEnter()
    {
        if (Context.JsModule is null) return;
        try
        {
            _scrollTimerId = await Context.JsModule.InvokeAsync<int>(
                "startScrollArrow", Context.PopupId, "down");
        }
        catch (Microsoft.JSInterop.JSDisconnectedException) { }
        catch (OperationCanceledException) { }
    }

    private async Task OnMouseLeave()
    {
        if (Context.JsModule is null || _scrollTimerId is null) return;
        try
        {
            await Context.JsModule.InvokeVoidAsync("stopScrollArrow", _scrollTimerId);
            _scrollTimerId = null;
        }
        catch (Microsoft.JSInterop.JSDisconnectedException) { }
        catch (OperationCanceledException) { }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Context.Open && Context.JsModule is not null)
        {
            try
            {
                _visible = await Context.JsModule.InvokeAsync<bool>(
                    "isScrollable", Context.PopupId, "down");
                StateHasChanged();
            }
            catch (Microsoft.JSInterop.JSDisconnectedException) { }
            catch (OperationCanceledException) { }
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!_visible && !KeepMounted) return;
        base.BuildRenderTree(builder);
    }
}

public readonly record struct SelectScrollArrowState(bool Visible);
