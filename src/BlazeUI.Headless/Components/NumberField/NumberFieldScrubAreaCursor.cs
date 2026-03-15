using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.NumberField;

/// <summary>
/// Optional virtual cursor rendered during pointer-lock scrubbing. Positioned
/// at the scrub point via <c>position: fixed</c> and <c>transform: translate3d()</c>,
/// updated directly by JS for performance.
/// </summary>
/// <remarks>
/// Only renders when actively scrubbing and pointer lock is granted (not on WebKit
/// or touch devices). Place inside <see cref="NumberFieldScrubArea"/>.
/// </remarks>
public class NumberFieldScrubAreaCursor : BlazeElement<NumberFieldScrubAreaCursorState>
{
    [CascadingParameter] internal NumberFieldScrubAreaContext ScrubContext { get; set; } = default!;
    [CascadingParameter] internal NumberFieldContext Context { get; set; } = default!;

    private string _cursorId = "";

    protected override string DefaultTag => "span";

    protected override void OnInitialized()
    {
        _cursorId = IdGenerator.Next("numberfield-scrub-cursor");
    }

    protected override string ElementId => _cursorId;

    protected override NumberFieldScrubAreaCursorState GetCurrentState() =>
        new(ScrubContext.IsScrubbing, Context.Disabled, Context.ReadOnly);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-scrubbing", ScrubContext.IsScrubbing ? "" : null);
        yield return new("data-disabled", Context.Disabled ? "" : null);
        yield return new("data-readonly", Context.ReadOnly ? "" : null);
        yield return new("data-required", Context.Required ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("role", "presentation");
        yield return new("style", "position: fixed; top: 0; left: 0; pointer-events: none;");
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Only render when actively scrubbing.
        if (!ScrubContext.IsScrubbing) return;
        base.BuildRenderTree(builder);
    }
}

public readonly record struct NumberFieldScrubAreaCursorState(bool Scrubbing, bool Disabled, bool ReadOnly);
