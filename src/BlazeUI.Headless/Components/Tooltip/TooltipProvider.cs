using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Tooltip;

/// <summary>
/// Coordinates delay behavior across multiple <see cref="TooltipRoot"/> instances.
/// When one tooltip closes and another opens within the <see cref="Timeout"/> window,
/// the second tooltip opens instantly (no open delay).
/// </summary>
/// <remarks>
/// Wrap multiple <see cref="TooltipRoot"/> instances in a single <c>TooltipProvider</c>.
/// Each child <see cref="TooltipRoot"/> checks for a cascaded <see cref="TooltipProviderContext"/>
/// and defers to its delay values when present, falling back to its own parameters.
/// </remarks>
public class TooltipProvider : ComponentBase
{
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Default delay in ms before showing tooltips. Individual <see cref="TooltipRoot"/>
    /// <c>Delay</c> parameters override this when explicitly set.
    /// </summary>
    [Parameter] public int? Delay { get; set; }

    /// <summary>
    /// Default delay in ms before hiding tooltips. Individual <see cref="TooltipRoot"/>
    /// <c>CloseDelay</c> parameters override this when explicitly set.
    /// </summary>
    [Parameter] public int? CloseDelay { get; set; }

    /// <summary>
    /// Window in ms after one tooltip closes during which the next tooltip opens instantly
    /// (skipping the open delay). Defaults to 400.
    /// </summary>
    [Parameter] public int Timeout { get; set; } = 400;

    private readonly TooltipProviderContext _context = new();

    protected override void OnParametersSet()
    {
        _context.Delay = Delay;
        _context.CloseDelay = CloseDelay;
        _context.Timeout = Timeout;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<TooltipProviderContext>>(0);
        builder.AddComponentParameter(1, "Value", _context);
        builder.AddComponentParameter(2, "ChildContent", ChildContent);
        builder.CloseComponent();
    }
}

public readonly record struct TooltipProviderState;
