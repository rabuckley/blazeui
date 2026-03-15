using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.NavigationMenu;

public class NavigationMenuItem : BlazeElement<NavigationMenuItemState>
{
    [CascadingParameter] internal NavigationMenuContext Context { get; set; } = default!;

    [Parameter, EditorRequired] public string Value { get; set; } = "";

    private string _triggerId = "";
    private string _contentId = "";

    protected override string DefaultTag => "li";

    protected override void OnInitialized()
    {
        _triggerId = IdGenerator.Next("navmenu-trigger");
        _contentId = IdGenerator.Next("navmenu-content");
        Context.RegisterItem(Value, _triggerId, _contentId);
    }

    private bool IsActive => Context.ActiveValue == Value;

    protected override NavigationMenuItemState GetCurrentState() => new(IsActive);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-active", IsActive ? "" : null);
        yield return new("data-value", Value);
    }
}

public readonly record struct NavigationMenuItemState(bool Active);
