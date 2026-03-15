using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.NumberField;

/// <summary>
/// Drag-to-adjust zone for number values via pointer lock. The user clicks and drags
/// horizontally or vertically to increment/decrement the value. Uses the Pointer Lock
/// API (<c>requestPointerLock()</c>) for continuous scrubbing without cursor boundaries.
/// </summary>
/// <remarks>
/// <para>
/// Pointer Lock is not available on WebKit (Safari) due to a layout-shift notification.
/// On WebKit and touch devices, the scrub area falls back to tracking raw pointer movement
/// without lock, which means the cursor hits screen edges.
/// </para>
/// <para>
/// This is a JS-heavy feature. The C# component manages lifecycle and cascades context;
/// all pointer tracking, delta accumulation, and value stepping happen in the JS module.
/// </para>
/// </remarks>
public class NumberFieldScrubArea : BlazeElement<NumberFieldScrubAreaState>
{
    [CascadingParameter] internal NumberFieldContext Context { get; set; } = default!;

    /// <summary>Scrub axis. Defaults to <c>"horizontal"</c>.</summary>
    [Parameter] public string Direction { get; set; } = "horizontal";

    /// <summary>
    /// Number of pixels the pointer must move before the value changes by one step.
    /// Lower values increase sensitivity. Defaults to 2.
    /// </summary>
    [Parameter] public int PixelSensitivity { get; set; } = 2;

    /// <summary>
    /// Distance in pixels at which the virtual cursor teleports to the opposite edge
    /// of the viewport. When null, teleportation is disabled.
    /// </summary>
    [Parameter] public int? TeleportDistance { get; set; }

    private string _scrubAreaId = "";
    private readonly NumberFieldScrubAreaContext _scrubContext = new();
    private bool _scrubAreaRegistered;

    protected override string DefaultTag => "span";

    protected override void OnInitialized()
    {
        _scrubAreaId = IdGenerator.Next("numberfield-scrub-area");
        _scrubContext.ScrubAreaId = _scrubAreaId;
        _scrubContext.Direction = Direction;
    }

    protected override string ElementId => _scrubAreaId;

    protected override NumberFieldScrubAreaState GetCurrentState() =>
        new(_scrubContext.IsScrubbing, Context.Disabled, Context.ReadOnly);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-scrubbing", _scrubContext.IsScrubbing ? "" : null);
        yield return new("data-disabled", Context.Disabled ? "" : null);
        yield return new("data-readonly", Context.ReadOnly ? "" : null);
        yield return new("data-required", Context.Required ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("role", "presentation");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_scrubAreaRegistered || Context.JsModule is null || Context.DotNetRef is null) return;
        if (Context.Disabled || Context.ReadOnly) return;

        _scrubAreaRegistered = true;

        try
        {
            await Context.JsModule.InvokeVoidAsync("initScrubArea",
                _scrubAreaId,
                Context.DotNetRef,
                new
                {
                    direction = Direction,
                    pixelSensitivity = PixelSensitivity,
                    teleportDistance = TeleportDistance,
                    step = Context.Step,
                    largeStep = Context.LargeStep,
                },
                Context.InstanceKey);
        }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }
    }

    /// <summary>
    /// Called by JS when a scrub gesture starts. Updates both the local scrub context
    /// (for ScrubAreaCursor rendering) and the root context (for data-scrubbing on Root/Group).
    /// </summary>
    [JSInvokable]
    public void OnScrubStart()
    {
        _scrubContext.IsScrubbing = true;
        Context.SetScrubbing();
        StateHasChanged();
    }

    /// <summary>Called by JS when the scrub gesture ends.</summary>
    [JSInvokable]
    public void OnScrubEnd()
    {
        _scrubContext.IsScrubbing = false;
        Context.ClearScrubbing();
        StateHasChanged();
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

        // The touch-action and user-select styles are required for correct scrubbing
        // behaviour on touch devices and to prevent text selection during drag.
        var mergedStyle = Css.Cn("touch-action: none; user-select: none; cursor: ew-resize;", Style, StyleBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedStyle))
            builder.AddAttribute(4, "style", mergedStyle);

        foreach (var attr in GetDataAttributes())
            if (attr.Value is not null)
                builder.AddAttribute(5, attr.Key, attr.Value);

        builder.AddAttribute(6, "role", "presentation");

        // Cascade scrub context to children (for ScrubAreaCursor).
        builder.OpenComponent<CascadingValue<NumberFieldScrubAreaContext>>(7);
        builder.AddComponentParameter(8, "Value", _scrubContext);
        builder.AddComponentParameter(9, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();
    }
}

public readonly record struct NumberFieldScrubAreaState(bool Scrubbing, bool Disabled, bool ReadOnly);
