using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Tabs;

/// <summary>
/// An individual tab control. Renders a <c>&lt;button&gt;</c> element with
/// <c>role="tab"</c> and roving tabindex focus management.
/// </summary>
public class TabsTab : BlazeElement<TabsTabState>
{
    [CascadingParameter] internal TabsContext Context { get; set; } = default!;

    /// <summary>
    /// The value that identifies this tab and its associated <see cref="TabsPanel"/>.
    /// </summary>
    [Parameter, EditorRequired] public string Value { get; set; } = "";

    /// <summary>
    /// Whether this tab is disabled. Disabled tabs cannot be activated via pointer or
    /// keyboard but remain focusable.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    protected override string DefaultTag => "button";

    // ElementId is overridden to use the context-registered ID so that
    // TabsPanel can reference it via aria-labelledby. Consumer Id takes
    // precedence for stable testing/ARIA IDs.
    protected override string ElementId => Id ?? _tabId;
    private string _tabId = "";

    protected override void OnInitialized()
    {
        _tabId = IdGenerator.Next("tab");
        Context.RegisterTab(Value, _tabId);
    }

    protected override void OnParametersSet()
    {
        // If the consumer provided an explicit Id, propagate it into context so
        // TabsPanel's aria-labelledby points at the right element.
        Context.RegisterTab(Value, Id ?? _tabId);
    }

    private bool IsActive => Context.ActiveValue == Value;

    protected override TabsTabState GetCurrentState() => new(IsActive, Disabled);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        var orientationValue = Context.Orientation is Orientation.Horizontal ? "horizontal" : "vertical";
        yield return new("data-active", IsActive ? "" : null);
        yield return new("data-disabled", Disabled ? "" : null);
        yield return new("data-orientation", orientationValue);
        // data-value is not part of Base UI's public data attributes but is needed by
        // the indicator JS to locate the active tab element within the tab list.
        yield return new("data-value", Value);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("role", "tab");
        yield return new("aria-selected", IsActive ? "true" : "false");
        yield return new("aria-controls", Context.GetPanelId(Value));
        yield return new("tabindex", IsActive ? "0" : "-1");

        if (Disabled)
            yield return new("disabled", true);

        yield return new("onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () =>
        {
            if (!Disabled) return Context.Activate(Value);
            return Task.CompletedTask;
        }));
    }
}

public readonly record struct TabsTabState(bool Active, bool Disabled);
