using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Popover;

/// <summary>
/// A button that closes the containing popover when clicked.
/// Renders a <c>&lt;button&gt;</c> element.
/// </summary>
public class PopoverClose : BlazeElement<PopoverCloseState>
{
    [CascadingParameter]
    internal PopoverContext Context { get; set; } = default!;

    protected override string DefaultTag => "button";

    protected override PopoverCloseState GetCurrentState() => default;

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield break;
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("onclick",
            EventCallback.Factory.Create<MouseEventArgs>(this, () => Context.SetOpen(false)));
    }
}

public readonly record struct PopoverCloseState;
