using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.NavigationMenu;

public class NavigationMenuLink : BlazeElement<NavigationMenuLinkState>
{
    [CascadingParameter] internal NavigationMenuContext Context { get; set; } = default!;

    /// <summary>
    /// Whether this link represents the current page. When <c>true</c>,
    /// <c>aria-current="page"</c> is applied to the anchor element.
    /// </summary>
    [Parameter] public bool Active { get; set; }

    /// <summary>
    /// Whether clicking this link closes the navigation menu. Defaults to <c>false</c>,
    /// matching Base UI's <c>closeOnClick</c> default.
    /// </summary>
    [Parameter] public bool CloseOnClick { get; set; } = false;

    protected override string DefaultTag => "a";
    protected override NavigationMenuLinkState GetCurrentState() => new(Active);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-active", Active ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        if (Active)
            yield return new("aria-current", "page");

        if (CloseOnClick)
            yield return new("onclick",
                EventCallback.Factory.Create<MouseEventArgs>(this, () =>
                    Context.SetActive(null)));
    }
}

public readonly record struct NavigationMenuLinkState(bool Active);
