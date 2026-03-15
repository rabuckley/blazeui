using BlazeUI.Bridge;
using BlazeUI.Headless.Core;
using BlazeUI.Headless.Overlay.Mutations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Popover;

/// <summary>
/// Positions the popover against the trigger. Renders a <c>&lt;div&gt;</c> element.
/// </summary>
public class PopoverPositioner : BlazeElement<PopoverPositionerState>
{
    [CascadingParameter]
    internal PopoverContext Context { get; set; } = default!;

    [Inject] internal BrowserMutationQueue MutationQueue { get; set; } = default!;

    [Parameter] public Side Side { get; set; } = Side.Bottom;
    [Parameter] public int SideOffset { get; set; } = 8;
    [Parameter] public Align Align { get; set; } = Align.Center;
    [Parameter] public int AlignOffset { get; set; }

    private string _resolvedSide = "bottom";
    private string _resolvedAlign = "center";
    private bool _wasOpen;

    protected override string DefaultTag => "div";

    protected override PopoverPositionerState GetCurrentState() => new(Context.Open, _resolvedSide, _resolvedAlign);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        // data-open / data-closed mirror Base UI's popupStateMapping applied to the positioner.
        yield return new("data-open", Context.Open ? "" : null);
        yield return new("data-closed", !Context.Open ? "" : null);
        yield return new("data-side", _resolvedSide);
        yield return new("data-align", _resolvedAlign);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Enqueue show only when transitioning closed->open for the first time after JS is ready.
        // The Root's OnBeforeCloseAsync enqueues the hide mutation, so we do not duplicate it here.
        if (Context.Open && !_wasOpen && Context.JsModule is not null)
        {
            MutationQueue.Enqueue(new ShowPositionedMutation
            {
                ElementId = Context.PopupId,
                AnchorId = Context.TriggerId,
                JsModule = Context.JsModule,
                Options = new
                {
                    placement = PlacementHelper.ToPlacement(Side, Align),
                    offset = SideOffset,
                },
                DotNetRef = Context.DotNetRef!,
            });
        }

        _wasOpen = Context.Open;
        await MutationQueue.FlushAsync();
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddAttribute(1, "id", ResolvedId);

        if (AdditionalAttributes is not null)
            builder.AddMultipleAttributes(2, AdditionalAttributes);

        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass))
            builder.AddAttribute(3, "class", mergedClass);

        builder.AddAttribute(4, "style", Css.Cn("position: absolute;", Style, StyleBuilder?.Invoke(state)));

        foreach (var attr in GetDataAttributes())
            if (attr.Value is not null)
                builder.AddAttribute(5, attr.Key, attr.Value);

        builder.AddContent(6, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct PopoverPositionerState(bool Open, string Side, string Align);
