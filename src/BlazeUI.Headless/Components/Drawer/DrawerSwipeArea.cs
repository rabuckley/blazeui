using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Drawer;

/// <summary>
/// An invisible area that listens for swipe gestures to open the drawer.
/// Renders a <c>&lt;div&gt;</c> element with <c>role="presentation"</c>.
/// </summary>
/// <remarks>
/// Swipe gesture tracking requires low-latency pointer event processing.
/// In Blazor Server mode, the network round-trip introduces latency that may
/// make gesture tracking impractical for smooth interaction. The swipe area
/// functions best in WebAssembly mode. In Server mode, it still responds to
/// swipe gestures but the visual feedback during the drag may lag.
/// </remarks>
public class DrawerSwipeArea : BlazeElement<DrawerSwipeAreaState>
{
    [CascadingParameter] internal DrawerContext Context { get; set; } = default!;

    /// <summary>
    /// When <c>true</c>, the swipe area does not respond to gestures.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// The direction that opens the drawer.
    /// Defaults to the opposite of the root's <c>SwipeDirection</c> (which is the dismiss direction).
    /// </summary>
    [Parameter] public SwipeDirection? SwipeDirection { get; set; }

    private string _swipeAreaId = "";

    protected override string DefaultTag => "div";

    protected override void OnInitialized()
    {
        _swipeAreaId = IdGenerator.Next("drawer-swipe-area");
    }

    protected override string ElementId => _swipeAreaId;

    // The dismiss direction comes from the root context. The open direction is the opposite.
    private SwipeDirection ResolvedSwipeDirection => SwipeDirection ?? Opposite(Context.SwipeDirection);

    private static SwipeDirection Opposite(SwipeDirection d) => d switch
    {
        Core.SwipeDirection.Up => Core.SwipeDirection.Down,
        Core.SwipeDirection.Down => Core.SwipeDirection.Up,
        Core.SwipeDirection.Left => Core.SwipeDirection.Right,
        Core.SwipeDirection.Right => Core.SwipeDirection.Left,
        _ => Core.SwipeDirection.Up,
    };

    protected override DrawerSwipeAreaState GetCurrentState() =>
        new(Context.Open, ResolvedSwipeDirection.ToString().ToLowerInvariant(), Disabled);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-open", Context.Open ? "" : null);
        yield return new("data-closed", !Context.Open ? "" : null);
        yield return new("data-swipe-direction", ResolvedSwipeDirection.ToString().ToLowerInvariant());
        yield return new("data-disabled", Disabled ? "" : null);
        // data-swiping is set/removed by JS during active pointer tracking.
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("role", "presentation");
        yield return new("aria-hidden", "true");
        yield return new("style", "touch-action: none; user-select: none;");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !Disabled && Context.JsModule is not null)
        {
            try
            {
                await Context.JsModule.InvokeVoidAsync("initSwipeArea",
                    _swipeAreaId,
                    Context.PopupId,
                    ResolvedSwipeDirection.ToString().ToLowerInvariant(),
                    Context.DotNetRef);
            }
            catch (JSDisconnectedException) { }
            catch (OperationCanceledException) { }
        }
    }
}

public readonly record struct DrawerSwipeAreaState(bool Open, string SwipeDirection, bool Disabled);
