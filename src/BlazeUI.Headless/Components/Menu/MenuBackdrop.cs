using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Menu;

/// <summary>
/// An overlay displayed beneath the menu popup.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
public class MenuBackdrop : BlazeElement<MenuBackdropState>
{
    [CascadingParameter] internal MenuContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";
    protected override MenuBackdropState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-open", Context.Open ? "" : null);
        yield return new("data-closed", !Context.Open ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        // role="presentation" matches Base UI — the backdrop is decorative.
        yield return new("role", "presentation");
    }
}

public readonly record struct MenuBackdropState(bool Open);
