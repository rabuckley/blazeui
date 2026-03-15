using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Tooltip;

public class TooltipArrow : BlazeElement<TooltipArrowState>
{
    [CascadingParameter]
    internal TooltipContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";

    protected override TooltipArrowState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-open", Context.Open ? "" : null);
        yield return new("data-closed", !Context.Open ? "" : null);
        yield return new("data-side", Context.Side);
        yield return new("data-align", Context.Align);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        // Arrows are decorative positioning elements; they are hidden from the
        // accessibility tree, matching Base UI's `aria-hidden: true` on TooltipArrow.
        yield return new("aria-hidden", "true");
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var attrs = BuildAttributes(state);

        // Arrow is absolutely positioned within the positioner.
        // Merge positioning style with any consumer-supplied style.
        var baseStyle = "position: absolute; left: 50%; transform: translateX(-50%);";
        if (attrs.TryGetValue("style", out var existing) && existing is string s)
            attrs["style"] = Css.Cn(baseStyle, s);
        else
            attrs["style"] = baseStyle;

        var tag = As ?? DefaultTag;
        builder.OpenElement(0, tag);
        builder.AddMultipleAttributes(1, attrs);
        builder.AddContent(2, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct TooltipArrowState(bool Open);
