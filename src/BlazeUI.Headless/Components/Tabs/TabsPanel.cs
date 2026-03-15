using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Tabs;

/// <summary>
/// Content panel associated with a <see cref="TabsTab"/>. Renders a <c>&lt;div&gt;</c>
/// element with <c>role="tabpanel"</c>. Hidden panels are removed from the DOM by
/// default; set <see cref="KeepMounted"/> to retain them.
/// </summary>
public class TabsPanel : BlazeElement<TabsPanelState>
{
    [CascadingParameter] internal TabsContext Context { get; set; } = default!;

    /// <summary>
    /// The value matching the <see cref="TabsTab.Value"/> that controls this panel.
    /// </summary>
    [Parameter, EditorRequired] public string Value { get; set; } = "";

    /// <summary>
    /// When <c>true</c>, the panel remains in the DOM when hidden. Useful when you need
    /// to preserve component state across tab switches. Defaults to <c>false</c>.
    /// </summary>
    [Parameter] public bool KeepMounted { get; set; }

    protected override string DefaultTag => "div";

    // The panel ID is deterministic from the value so that TabsTab can reference it
    // via aria-controls before the panel registers itself. Consumer Id takes
    // precedence for stable testing/ARIA IDs.
    protected override string ElementId => Id ?? _panelId;
    private string _panelId = "";
    private int _panelIndex;

    protected override void OnInitialized()
    {
        _panelId = $"blazeui-tabpanel-{Value}";
        _panelIndex = Context.RegisterPanel(Value, _panelId);
    }

    protected override void OnParametersSet()
    {
        // If the consumer provided an explicit Id, propagate it into context so
        // TabsTab's aria-controls points at the right element.
        Context.SetPanelId(Value, Id ?? _panelId);
    }

    private bool IsActive => Context.ActiveValue == Value;

    protected override TabsPanelState GetCurrentState() => new(IsActive);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        var orientationValue = Context.Orientation is Orientation.Horizontal ? "horizontal" : "vertical";
        yield return new("data-hidden", IsActive ? null : "");
        yield return new("data-orientation", orientationValue);
        yield return new("data-activation-direction", Context.ActivationDirection);
        yield return new("data-index", _panelIndex.ToString());
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("role", "tabpanel");
        // Active panels are in the tab order (tabIndex 0); inactive panels are removed
        // from it (tabIndex -1) so keyboard users skip over hidden content.
        yield return new("tabindex", IsActive ? "0" : "-1");

        // Establish the accessible name link from panel to its controlling tab.
        var tabId = Context.GetTabId(Value);
        if (tabId is not null)
            yield return new("aria-labelledby", tabId);

        if (!IsActive)
            yield return new("hidden", true);
    }

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        // When keepMounted is false (default), hidden panels are removed from the DOM
        // entirely so they don't contribute to accessibility trees or layout.
        if (!IsActive && !KeepMounted)
            return;

        base.BuildRenderTree(builder);
    }
}

public readonly record struct TabsPanelState(bool Active);
