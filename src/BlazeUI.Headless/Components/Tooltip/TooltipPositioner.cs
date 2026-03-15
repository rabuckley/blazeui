using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Tooltip;

public class TooltipPositioner : BlazeElement<TooltipPositionerState>
{
    [CascadingParameter]
    internal TooltipContext Context { get; set; } = default!;

    [Parameter] public Side Side { get; set; } = Side.Top;
    [Parameter] public int SideOffset { get; set; } = 0;
    [Parameter] public Align Align { get; set; } = Align.Center;
    [Parameter] public int AlignOffset { get; set; }

    private string _resolvedSide = "top";
    private string _resolvedAlign = "center";

    protected override string DefaultTag => "div";
    protected override string ElementId => Context.PositionerId;

    protected override TooltipPositionerState GetCurrentState() => new(_resolvedSide, _resolvedAlign);

    protected override void OnParametersSet()
    {
        var placement = PlacementHelper.ToPlacement(Side, Align);
        _resolvedSide = PlacementHelper.ToDataSide(placement);
        _resolvedAlign = PlacementHelper.ToDataAlign(placement);

        // Propagate side/align to context so popup and arrow can emit matching data attributes.
        Context.Side = _resolvedSide;
        Context.Align = _resolvedAlign;
    }

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-open", Context.Open ? "" : null);
        yield return new("data-closed", !Context.Open ? "" : null);
        yield return new("data-side", _resolvedSide);
        yield return new("data-align", _resolvedAlign);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Context is { Open: true, JsModule: not null })
        {
            await Context.JsModule.InvokeVoidAsync("positionAndShow",
                Context.TriggerId, Context.PositionerId, new
                {
                    placement = PlacementHelper.ToPlacement(Side, Align),
                    offset = SideOffset,
                });
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddAttribute(1, "id", ElementId);

        if (AdditionalAttributes is not null)
            builder.AddMultipleAttributes(2, AdditionalAttributes);

        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass))
            builder.AddAttribute(3, "class", mergedClass);

        builder.AddAttribute(4, "style", Css.Cn("position: absolute;", Style, StyleBuilder?.Invoke(state)));

        foreach (var attr in GetDataAttributes())
            if (attr.Value is not null)
                builder.AddAttribute(5, attr.Key, attr.Value);

        builder.AddAttribute(6, "role", "presentation");

        builder.AddContent(7, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct TooltipPositionerState(string Side, string Align);
