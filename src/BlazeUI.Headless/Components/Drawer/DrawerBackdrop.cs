using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Drawer;

public class DrawerBackdrop : BlazeElement<DrawerBackdropState>
{
    [CascadingParameter] internal DrawerContext Context { get; set; } = default!;

    // Only mount the backdrop after the drawer has been opened at least once.
    // Without this guard the backdrop renders into the PortalHost on initial
    // load, producing a visible overlay before any interaction.
    private bool _mounted;

    protected override string DefaultTag => "div";
    protected override DrawerBackdropState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-open", Context.Open ? "" : null);
        yield return new("data-closed", !Context.Open ? "" : null);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Context.Open) _mounted = true;
        if (!_mounted) return;

        // Keep rendering with data-closed on every re-render so Blazor's
        // diff is a no-op and preserves JS-set display:none. JS hide()
        // onComplete sets display:none on the backdrop after the CSS
        // fade-out animation finishes.
        base.BuildRenderTree(builder);
    }
}

public readonly record struct DrawerBackdropState(bool Open);
