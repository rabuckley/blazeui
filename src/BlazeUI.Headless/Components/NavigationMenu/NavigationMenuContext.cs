using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.NavigationMenu;

internal sealed class NavigationMenuContext
{
    public string? ActiveValue { get; set; }
    public string? PreviousValue { get; set; }
    public Orientation Orientation { get; set; } = Orientation.Horizontal;

    /// <summary>
    /// The direction from which the last trigger was activated. Used to set
    /// <c>data-activation-direction</c> on <see cref="NavigationMenuContent"/>.
    /// </summary>
    public string? ActivationDirection { get; set; }

    public Func<string?, Task> SetActive { get; set; } = _ => Task.CompletedTask;
    public IJSObjectReference? JsModule { get; set; }
    public DotNetObjectReference<NavigationMenuRoot>? DotNetRef { get; set; }
    public string RootId { get; set; } = "";
    public string ListId { get; set; } = "";
    public string ViewportId { get; set; } = "";

    private readonly Dictionary<string, string> _triggerIds = new();
    private readonly Dictionary<string, string> _contentIds = new();
    private readonly List<string> _itemOrder = new();

    // Content fragments registered by NavigationMenuContent, rendered by NavigationMenuViewport.
    // This is the Blazor equivalent of React's createPortal — content is authored inside
    // NavigationMenuItem but rendered inside NavigationMenuViewport so the viewport can
    // animate its size around the active content panel.
    private readonly Dictionary<string, RenderFragment> _contentFragments = new();

    /// <summary>
    /// Invoked when a content fragment is registered or unregistered, so
    /// <see cref="NavigationMenuViewport"/> can re-render to pick up the change.
    /// </summary>
    public Action? OnContentChanged { get; set; }

    public void RegisterItem(string value, string triggerId, string contentId)
    {
        _triggerIds[value] = triggerId;
        _contentIds[value] = contentId;
        if (!_itemOrder.Contains(value))
            _itemOrder.Add(value);
    }

    /// <summary>
    /// Computes the activation direction based on the DOM order of items.
    /// Returns "left" when moving to an earlier item, "right" when moving to a later one.
    /// </summary>
    public string? ComputeDirection(string? from, string? to)
    {
        if (from is null || to is null) return null;
        var fromIndex = _itemOrder.IndexOf(from);
        var toIndex = _itemOrder.IndexOf(to);
        if (fromIndex < 0 || toIndex < 0) return null;
        return toIndex > fromIndex ? "right" : "left";
    }

    public string? GetTriggerId(string value) =>
        _triggerIds.TryGetValue(value, out var id) ? id : null;

    public string? GetContentId(string value) =>
        _contentIds.TryGetValue(value, out var id) ? id : null;

    public void RegisterContentFragment(string value, RenderFragment fragment)
    {
        _contentFragments[value] = fragment;
        OnContentChanged?.Invoke();
    }

    public void UnregisterContentFragment(string value)
    {
        _contentFragments.Remove(value);
        OnContentChanged?.Invoke();
    }

    public RenderFragment? GetContentFragment(string value) =>
        _contentFragments.TryGetValue(value, out var fragment) ? fragment : null;
}
