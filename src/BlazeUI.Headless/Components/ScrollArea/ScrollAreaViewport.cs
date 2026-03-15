using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.ScrollArea;

/// <summary>
/// The actual scrollable container of the scroll area.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
/// <remarks>
/// <para>
/// Native scrollbars are suppressed with <c>scrollbar-width: none</c> so that the
/// custom BlazeUI scrollbars are the only visible scroll indicators.
/// </para>
/// <para>
/// A <c>tabindex</c> of <c>0</c> keeps the viewport keyboard-accessible when content
/// overflows in any direction. The JS module sets <c>tabindex=-1</c> when no overflow
/// is present, matching the Base UI accessibility behaviour described at
/// https://accessibilityinsights.io/info-examples/web/scrollable-region-focusable/.
/// </para>
/// <para>
/// The <c>data-id</c> attribute namespaces this element so the JS module can locate it
/// alongside the scrollbars and corner without relying on generated IDs.
/// </para>
/// </remarks>
public class ScrollAreaViewport : BlazeElement<ScrollAreaViewportState>
{
    [CascadingParameter] internal ScrollAreaContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";
    protected override ScrollAreaViewportState GetCurrentState() => new();
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes() => [];

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddAttribute(1, "id", Context.ViewportId);
        if (AdditionalAttributes is not null) builder.AddMultipleAttributes(2, AdditionalAttributes);
        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass)) builder.AddAttribute(3, "class", mergedClass);

        // overflow: scroll exposes scroll position even when content fits so the JS module can
        // always compute thumb positions. scrollbar-width: none hides native scrollbars in
        // Firefox and Chrome; the ::-webkit-scrollbar rule in the headless stylesheet covers Safari.
        var baseStyle = "overflow: scroll; scrollbar-width: none;";
        var userStyle = Css.Cn(Style, StyleBuilder?.Invoke(state));
        var mergedStyle = string.IsNullOrEmpty(userStyle) ? baseStyle : $"{baseStyle} {userStyle}";
        builder.AddAttribute(4, "style", mergedStyle);

        builder.AddAttribute(5, "role", "presentation");
        builder.AddAttribute(6, "data-id", $"{Context.RootId}-viewport");
        // Start keyboard-accessible; the JS module adjusts tabindex based on actual overflow.
        builder.AddAttribute(7, "tabindex", "0");

        builder.AddContent(8, ChildContent);
        builder.CloseElement();
    }
}

/// <summary>
/// State for <see cref="ScrollAreaViewport"/>. Scroll and overflow state is
/// managed by the JS module via direct DOM attribute mutations.
/// </summary>
public readonly record struct ScrollAreaViewportState;
