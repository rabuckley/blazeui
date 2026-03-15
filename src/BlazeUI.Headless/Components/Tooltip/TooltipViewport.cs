using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Tooltip;

/// <summary>
/// A viewport for displaying animated content transitions when a single tooltip is
/// shared across multiple triggers and the content changes when switching between them.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
/// <remarks>
/// Only required when one <see cref="TooltipRoot"/> can be opened by multiple triggers
/// (via detached handles) and content transitions between triggers need animation.
/// For the typical single-trigger case, use <see cref="TooltipPopup"/> directly
/// inside <see cref="TooltipPositioner"/> without a viewport wrapper.
/// <para>
/// TODO: content transition animation (data-current/data-previous/data-activation-direction
/// and data-transitioning) is not yet implemented — the component renders a plain wrapper.
/// </para>
/// </remarks>
public class TooltipViewport : BlazeElement<TooltipViewportState>
{
    [CascadingParameter]
    internal TooltipContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";

    protected override TooltipViewportState GetCurrentState() => new();

    // No data attributes: the activation-direction and transitioning attributes
    // are emitted by the JS-driven content transition logic (deferred).
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
        => [];
}

public readonly record struct TooltipViewportState;
