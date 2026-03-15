using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Popover;

/// <summary>
/// A heading that labels the popover. Renders an <c>&lt;h2&gt;</c> element.
/// </summary>
public class PopoverTitle : BlazeElement<PopoverTitleState>
{
    [CascadingParameter]
    internal PopoverContext Context { get; set; } = default!;

    protected override string DefaultTag => "h2";

    protected override PopoverTitleState GetCurrentState() => default;

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield break;
    }

    protected override void OnInitialized()
    {
        // Register this element's ID with the root context so PopoverPopup can emit
        // aria-labelledby pointing to this heading.
        Context.TitleId = ResolvedId;
    }
}

public readonly record struct PopoverTitleState;
