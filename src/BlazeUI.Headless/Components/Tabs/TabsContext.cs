using BlazeUI.Headless.Core;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Tabs;

internal sealed class TabsContext
{
    public string? ActiveValue { get; set; }
    public string? PreviousValue { get; set; }
    public string ActivationDirection { get; set; } = "none";
    public Orientation Orientation { get; set; } = Orientation.Horizontal;
    public Func<string, Task> Activate { get; set; } = _ => Task.CompletedTask;
    public IJSObjectReference? JsModule { get; set; }
    public DotNetObjectReference<TabsRoot>? DotNetRef { get; set; }
    public string TabListId { get; set; } = "";

    // Track registration order for activation direction calculation and
    // maintain the mapping from tab value to element IDs for ARIA wiring.
    private readonly List<string> _registeredValues = new();
    private readonly Dictionary<string, string> _tabIdByValue = new();
    private readonly Dictionary<string, string> _panelIdByValue = new();
    private int _nextPanelIndex;

    public int RegisterTab(string value, string tabId)
    {
        if (!_registeredValues.Contains(value))
            _registeredValues.Add(value);
        _tabIdByValue[value] = tabId;
        return _registeredValues.IndexOf(value);
    }

    public int RegisterPanel(string value, string panelId)
    {
        _panelIdByValue[value] = panelId;
        return _nextPanelIndex++;
    }

    /// <summary>
    /// Updates the panel ID for a previously registered value without allocating
    /// a new panel index. Used when a consumer overrides the panel's <c>Id</c>.
    /// </summary>
    public void SetPanelId(string value, string panelId) =>
        _panelIdByValue[value] = panelId;

    public string? GetTabId(string value) =>
        _tabIdByValue.TryGetValue(value, out var id) ? id : null;

    public string GetPanelId(string value)
    {
        if (_panelIdByValue.TryGetValue(value, out var id)) return id;
        // Deterministic fallback so Tab can reference a Panel before the Panel registers.
        return $"blazeui-tabpanel-{value}";
    }

    public int GetIndex(string value) => _registeredValues.IndexOf(value);
}
