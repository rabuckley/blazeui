using BlazeUI.Headless.Core;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Menubar;

internal sealed class MenubarContext
{
    public string RootId { get; set; } = "";
    public string? ActiveMenu { get; set; }
    public bool Modal { get; set; } = true;
    public bool Disabled { get; set; }
    public Orientation Orientation { get; set; } = Orientation.Horizontal;
    public bool HasSubmenuOpen { get; set; }

    public Func<string, Task> OpenMenu { get; set; } = _ => Task.CompletedTask;
    public Func<Task> CloseAll { get; set; } = () => Task.CompletedTask;
    public Action<bool> SetHasSubmenuOpen { get; set; } = _ => { };
    public IJSObjectReference? JsModule { get; set; }
    public DotNetObjectReference<MenubarRoot>? DotNetRef { get; set; }

    private readonly List<string> _menuValues = new();
    private readonly Dictionary<string, string> _triggerIds = new();

    public void RegisterMenu(string value, string triggerId)
    {
        if (!_menuValues.Contains(value))
            _menuValues.Add(value);
        _triggerIds[value] = triggerId;
    }

    public IReadOnlyList<string> MenuValues => _menuValues;

    public string? GetAdjacentMenu(string current, int direction)
    {
        var index = _menuValues.IndexOf(current);
        if (index == -1) return null;
        var newIndex = (index + direction + _menuValues.Count) % _menuValues.Count;
        return _menuValues[newIndex];
    }

    public string? GetTriggerIdForMenu(string menuValue)
    {
        return _triggerIds.TryGetValue(menuValue, out var id) ? id : null;
    }
}
