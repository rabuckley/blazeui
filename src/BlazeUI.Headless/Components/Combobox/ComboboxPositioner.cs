using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Combobox;

public class ComboboxPositioner : BlazeElement<ComboboxPositionerState>
{
    [CascadingParameter] internal ComboboxContext Context { get; set; } = default!;

    [Parameter] public Side Side { get; set; } = Side.Bottom;
    [Parameter] public int SideOffset { get; set; } = 4;
    [Parameter] public Align Align { get; set; } = Align.Start;
    [Parameter] public int AlignOffset { get; set; }

    protected override string DefaultTag => "div";
    protected override string ElementId => Context.PositionerId;
    protected override ComboboxPositionerState GetCurrentState() => new(
        PlacementHelper.ToDataSide(PlacementHelper.ToPlacement(Side, Align)),
        PlacementHelper.ToDataAlign(PlacementHelper.ToPlacement(Side, Align)));

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        var state = GetCurrentState();
        yield return new("data-side", state.Side);
        yield return new("data-align", state.Align);
    }

    // Publish placement info to the context so the Popup can include it
    // in its ShowComboboxMutation. The Positioner owns the placement
    // parameters; the Popup owns the show/hide lifecycle.
    protected override void OnParametersSet()
    {
        Context.Placement = PlacementHelper.ToPlacement(Side, Align);
        Context.PlacementOffset = SideOffset;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddAttribute(1, "id", ResolvedId);
        if (AdditionalAttributes is not null) builder.AddMultipleAttributes(2, AdditionalAttributes);
        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass)) builder.AddAttribute(3, "class", mergedClass);
        builder.AddAttribute(4, "style", Css.Cn("position: absolute;", Style, StyleBuilder?.Invoke(state)));
        foreach (var attr in GetDataAttributes())
            if (attr.Value is not null) builder.AddAttribute(5, attr.Key, attr.Value);
        builder.AddAttribute(6, "role", "presentation");
        builder.AddContent(7, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct ComboboxPositionerState(string Side, string Align);
