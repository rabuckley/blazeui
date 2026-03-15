using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.ScrollArea;

/// <summary>
/// A vertical or horizontal scrollbar for the scroll area.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
/// <remarks>
/// <para>
/// The scrollbar is hidden (not rendered) when the viewport content fits in the
/// scroll direction, unless <see cref="KeepMounted"/> is <c>true</c>. This matches
/// Base UI's behaviour: the element is unmounted rather than hidden with CSS so that
/// it does not participate in layout.
/// </para>
/// <para>
/// State data attributes (<c>data-hovering</c>, <c>data-scrolling</c>,
/// <c>data-has-overflow-x/y</c>, overflow edge attributes) are applied and removed
/// directly by the JS module rather than through Blazor re-renders. This avoids
/// Blazor server round-trips for high-frequency scroll events.
/// </para>
/// <para>
/// The <c>data-visibility</c> attribute reflects <see cref="Visibility"/> so CSS can
/// apply mode-specific styling. <c>data-hovering</c> and <c>data-scrolling</c> are
/// managed by JS; CSS selectors combine these with <c>data-visibility</c> to control
/// fade-in/fade-out behaviour.
/// </para>
/// <para>
/// The CSS custom properties <c>--scroll-area-thumb-height</c> and
/// <c>--scroll-area-thumb-width</c> are written by the JS module onto this element
/// and consumed by the thumb's inline style.
/// </para>
/// </remarks>
public class ScrollAreaScrollbar : BlazeElement<ScrollAreaScrollbarState>
{
    [CascadingParameter] internal ScrollAreaContext Context { get; set; } = default!;

    /// <summary>Whether the scrollbar controls vertical or horizontal scroll.</summary>
    [Parameter] public Orientation Orientation { get; set; } = Orientation.Vertical;

    /// <summary>
    /// Controls when the scrollbar is visible. <see cref="ScrollbarVisibility.Hover"/>
    /// fades in on pointer hover or active scrolling, <see cref="ScrollbarVisibility.Scroll"/>
    /// only on active scrolling, and <see cref="ScrollbarVisibility.Always"/> keeps the
    /// scrollbar permanently visible. <see cref="ScrollbarVisibility.Auto"/> behaves
    /// identically to <see cref="ScrollbarVisibility.Hover"/>.
    /// </summary>
    [Parameter] public ScrollbarVisibility Visibility { get; set; } = ScrollbarVisibility.Always;

    /// <summary>
    /// Whether to keep the HTML element in the DOM when the viewport is not scrollable
    /// in this direction. When <c>false</c> (default), the element is unmounted so it
    /// does not participate in layout.
    /// </summary>
    [Parameter] public bool KeepMounted { get; set; } = false;

    protected override string DefaultTag => "div";
    protected override ScrollAreaScrollbarState GetCurrentState() => new(Orientation);

    private string VisibilityValue => Visibility switch
    {
        ScrollbarVisibility.Always => "always",
        ScrollbarVisibility.Scroll => "scroll",
        // Auto behaves identically to Hover.
        _ => "hover",
    };

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        // data-orientation is always present so CSS selectors can target each scrollbar direction.
        yield return new("data-orientation", Orientation is Orientation.Horizontal ? "horizontal" : "vertical");
        yield return new("data-id", $"{Context.RootId}-scrollbar");
        yield return new("data-visibility", VisibilityValue);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddAttribute(1, "id", ElementId);
        if (AdditionalAttributes is not null) builder.AddMultipleAttributes(2, AdditionalAttributes);
        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass)) builder.AddAttribute(3, "class", mergedClass);

        // Scrollbar tracks are absolutely positioned relative to the root so they overlay content.
        // Orientation-specific CSS vars set by JS control thumb sizing.
        var positionStyle = Orientation is Orientation.Vertical
            ? "position: absolute; touch-action: none; user-select: none; top: 0; bottom: var(--scroll-area-corner-height); inset-inline-end: 0;"
            : "position: absolute; touch-action: none; user-select: none; inset-inline-start: 0; inset-inline-end: var(--scroll-area-corner-width); bottom: 0;";
        var userStyle = Css.Cn(Style, StyleBuilder?.Invoke(state));
        var mergedStyle = string.IsNullOrEmpty(userStyle) ? positionStyle : $"{positionStyle} {userStyle}";
        builder.AddAttribute(4, "style", mergedStyle);

        foreach (var attr in GetDataAttributes())
            if (attr.Value is not null) builder.AddAttribute(5, attr.Key, attr.Value);

        // Cascade orientation to the child thumb so it knows which CSS var to consume for sizing.
        builder.OpenComponent<CascadingValue<ScrollbarOrientationContext>>(6);
        builder.AddComponentParameter(7, "Value", new ScrollbarOrientationContext(Orientation));
        builder.AddComponentParameter(8, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();
    }
}

/// <summary>State for <see cref="ScrollAreaScrollbar"/>.</summary>
public readonly record struct ScrollAreaScrollbarState(Orientation Orientation);

/// <summary>
/// Cascaded from <see cref="ScrollAreaScrollbar"/> to <see cref="ScrollAreaThumb"/> so
/// the thumb knows which orientation-specific CSS variable to consume for sizing.
/// </summary>
internal sealed record ScrollbarOrientationContext(Orientation Orientation);
