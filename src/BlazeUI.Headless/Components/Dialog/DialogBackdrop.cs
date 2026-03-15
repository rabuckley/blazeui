using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Dialog;

public class DialogBackdrop : BlazeElement<DialogBackdropState>
{
    [CascadingParameter]
    internal DialogContext Context { get; set; } = default!;

    /// <summary>
    /// When <c>true</c>, renders the backdrop even when nested inside another
    /// dialog. By default, only the outermost dialog's backdrop renders.
    /// </summary>
    [Parameter] public bool ForceRender { get; set; }

    private bool _mounted;
    private bool _wasOpen;
    private bool _closedRendered;

    protected override string DefaultTag => "div";

    protected override DialogBackdropState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-open", Context.Open ? "" : null);
        yield return new("data-closed", !Context.Open ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        // Backdrop is a visual element only; screen readers should not interact with it.
        // aria-hidden and user-select:none match Base UI's inert mechanism which marks
        // overlay siblings as non-interactive while the dialog is open.
        yield return new("role", "presentation");
        yield return new("aria-hidden", "true");
        yield return new("style", "user-select: none;");
    }

    protected override void OnParametersSet()
    {
        if (Context.Open && !_wasOpen)
        {
            _mounted = true;
            _closedRendered = false;
        }
        _wasOpen = Context.Open;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!_mounted) return;
        if (Context.NestingLevel > 1 && !ForceRender) return;

        // Render once with data-closed to trigger the CSS exit animation,
        // then stop rendering so Blazor re-renders don't reset display:none
        // (set by JS) and restart the animation.
        if (!Context.Open && _closedRendered) return;
        if (!Context.Open) _closedRendered = true;

        base.BuildRenderTree(builder);
    }
}

public readonly record struct DialogBackdropState(bool Open);
