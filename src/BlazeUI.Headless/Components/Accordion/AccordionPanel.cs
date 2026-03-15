using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Accordion;

public class AccordionPanel : BlazeElement<AccordionPanelState>
{
    [CascadingParameter] internal AccordionContext Context { get; set; } = default!;
    [CascadingParameter] internal AccordionItemContext ItemContext { get; set; } = default!;

    protected override string DefaultTag => "div";
    protected override AccordionPanelState GetCurrentState() =>
        new(ItemContext.Open, ItemContext.Disabled, Context.Orientation, ItemContext.Index);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        // Only set data-closed on panels actively animating closed, not on
        // statically-hidden panels. Hidden panels with data-closed preload the
        // accordion-up animation property, which causes the browser to skip the
        // accordion-down animation when the panel opens (the animation property
        // swaps in the same frame as the display: none → visible transition).
        var isAnimatingClosed = Context.ClosingPanelIds.Contains(ItemContext.PanelId);
        yield return new("data-open", ItemContext.Open ? "" : null);
        yield return new("data-closed", isAnimatingClosed ? "" : null);
        yield return new("data-disabled", ItemContext.Disabled ? "" : null);
        yield return new("data-orientation", Context.Orientation is Orientation.Horizontal ? "horizontal" : "vertical");
        yield return new("data-index", ItemContext.Index.ToString());
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // If initially open on first render, measure the panel height so
        // the CSS custom property is available for transition styling.
        if (firstRender && ItemContext.Open && Context.JsModule is not null)
        {
            try { await Context.JsModule.InvokeVoidAsync("openPanel", ItemContext.PanelId, /* suppressAnimation */ true); }
            catch (JSDisconnectedException) { }
            catch (OperationCanceledException) { }
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddAttribute(1, "id", ItemContext.PanelId);
        if (AdditionalAttributes is not null) builder.AddMultipleAttributes(2, AdditionalAttributes);
        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass)) builder.AddAttribute(3, "class", mergedClass);
        var mergedStyle = Css.Cn(Style, StyleBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedStyle)) builder.AddAttribute(4, "style", mergedStyle);
        foreach (var attr in GetDataAttributes())
            if (attr.Value is not null) builder.AddAttribute(5, attr.Key, attr.Value);

        builder.AddAttribute(6, "role", "region");
        builder.AddAttribute(7, "aria-labelledby", ItemContext.TriggerId);

        // Only hide the panel if it's closed AND not mid-close-animation.
        // During the exit animation the panel must remain visible so the
        // CSS animation (accordion-up) can play.
        var isAnimatingClosed = Context.ClosingPanelIds.Contains(ItemContext.PanelId);
        if (!ItemContext.Open && !isAnimatingClosed)
            builder.AddAttribute(8, "hidden", true);

        builder.AddContent(9, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct AccordionPanelState(bool Open, bool Disabled, Orientation Orientation, int Index);
