using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.NavigationMenu;

/// <remarks>
/// Keeps its own <see cref="BuildRenderTree"/> because the trigger ID is conditional —
/// <c>Context.GetTriggerId(Value)</c> can return <c>null</c>, in which case the <c>id</c>
/// attribute must be omitted entirely.
/// </remarks>
public class NavigationMenuTrigger : BlazeElement<NavigationMenuTriggerState>
{
    [CascadingParameter] internal NavigationMenuContext Context { get; set; } = default!;

    [Parameter, EditorRequired] public string Value { get; set; } = "";

    protected override string DefaultTag => "button";

    private bool IsOpen => Context.ActiveValue == Value;
    private string? ContentId => Context.GetContentId(Value);

    protected override NavigationMenuTriggerState GetCurrentState() => new(IsOpen);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        // Base UI marks all triggers with this identifier so dismiss/hover logic can
        // exclude clicks on triggers from outside-press detection.
        yield return new("data-base-ui-navigation-menu-trigger", "");
        // data-popup-open and data-pressed are both present when the item is open
        // (Base UI's pressableTriggerOpenStateMapping sets both simultaneously).
        yield return new("data-popup-open", IsOpen ? "" : null);
        yield return new("data-pressed", IsOpen ? "" : null);
        // data-value is a BlazeUI convention used by the JS module for hover-intent
        // and keyboard navigation; it is not part of Base UI's public API.
        yield return new("data-value", Value);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("aria-expanded", IsOpen ? "true" : "false");

        if (ContentId is not null)
            yield return new("aria-controls", ContentId);

        yield return new("onclick",
            EventCallback.Factory.Create<MouseEventArgs>(this, () =>
                Context.SetActive(IsOpen ? null : Value)));
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var attrs = BuildAttributes(state);

        // The trigger ID is conditional — only render it when non-null.
        var triggerId = Context.GetTriggerId(Value);
        attrs.Remove("id");
        if (triggerId is not null)
            attrs["id"] = triggerId;

        if (Render is not null)
        {
            builder.AddContent(0, Render, new ElementProps(attrs, ChildContent));
        }
        else
        {
            var tag = As ?? DefaultTag;
            builder.OpenElement(0, tag);
            builder.AddMultipleAttributes(1, attrs);
            builder.AddContent(2, ChildContent);
            builder.CloseElement();
        }
    }
}

public readonly record struct NavigationMenuTriggerState(bool Open);
