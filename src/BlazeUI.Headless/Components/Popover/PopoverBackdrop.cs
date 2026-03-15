using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Popover;

/// <summary>
/// An overlay displayed beneath the popover. Renders a <c>&lt;div&gt;</c> element.
/// </summary>
public class PopoverBackdrop : BlazeElement<PopoverBackdropState>
{
    [CascadingParameter]
    internal PopoverContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";

    protected override PopoverBackdropState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-open", Context.Open ? "" : null);
        yield return new("data-closed", !Context.Open ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        // The backdrop is a purely visual overlay — hide it from the accessibility tree.
        yield return new("role", "presentation");
    }
}

public readonly record struct PopoverBackdropState(bool Open);
