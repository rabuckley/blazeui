using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Popover;

/// <summary>
/// A paragraph with additional information about the popover.
/// Renders a <c>&lt;p&gt;</c> element.
/// </summary>
public class PopoverDescription : BlazeElement<PopoverDescriptionState>
{
    [CascadingParameter]
    internal PopoverContext Context { get; set; } = default!;

    protected override string DefaultTag => "p";

    protected override PopoverDescriptionState GetCurrentState() => default;

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield break;
    }

    protected override void OnInitialized()
    {
        // Register this element's ID with the root context so PopoverPopup can emit
        // aria-describedby pointing to this paragraph.
        Context.DescriptionId = ResolvedId;
    }
}

public readonly record struct PopoverDescriptionState;
