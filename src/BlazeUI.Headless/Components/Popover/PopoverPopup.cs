using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Popover;

/// <summary>
/// A container for the popover contents. Renders a <c>&lt;div&gt;</c> element.
/// </summary>
public class PopoverPopup : BlazeElement<PopoverPopupState>
{
    [CascadingParameter]
    internal PopoverContext Context { get; set; } = default!;

    private bool _mounted;
    private bool _wasOpen;

    protected override string DefaultTag => "div";

    protected override PopoverPopupState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        // data-open/data-closed are owned exclusively by JS (show/hide) to avoid
        // Blazor triggering CSS entry animations before floating-ui positioning.
        yield break;
    }

    protected override void OnParametersSet()
    {
        if (Context.Open && !_wasOpen)
            _mounted = true;
        // Exit animation keeps _mounted true — JS calls OnExitAnimationComplete to unmount.

        _wasOpen = Context.Open;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!_mounted) return;

        var state = GetCurrentState();
        var attrs = BuildAttributes(state);

        // The popup element ID is set by the context so the trigger's aria-controls
        // and the positioner's JS calls can reference a stable, predictable value.
        attrs["id"] = Context.PopupId;

        // Base UI uses floating-ui's useRole hook, which resolves to "dialog" for popovers.
        attrs["role"] = "dialog";
        if (!string.IsNullOrEmpty(Context.TitleId))
            attrs["aria-labelledby"] = Context.TitleId;
        if (!string.IsNullOrEmpty(Context.DescriptionId))
            attrs["aria-describedby"] = Context.DescriptionId;

        var tag = As ?? DefaultTag;
        builder.OpenElement(0, tag);
        builder.AddMultipleAttributes(1, attrs);
        builder.AddContent(2, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct PopoverPopupState(bool Open);
