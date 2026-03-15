using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Accordion;

public class AccordionTrigger : BlazeElement<AccordionTriggerState>
{
    [CascadingParameter] internal AccordionContext Context { get; set; } = default!;
    [CascadingParameter] internal AccordionItemContext ItemContext { get; set; } = default!;

    protected override string DefaultTag => "button";
    protected override string ElementId => Id ?? ItemContext.TriggerId;

    protected override void OnParametersSet()
    {
        // Propagate consumer Id into context so AccordionPanel's aria-labelledby
        // points at the right element.
        if (Id is not null)
            ItemContext.TriggerId = Id;
    }
    protected override AccordionTriggerState GetCurrentState() => new(ItemContext.Open, ItemContext.Disabled, Context.Orientation, ItemContext.Index);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-panel-open", ItemContext.Open ? "" : null);
        yield return new("data-disabled", ItemContext.Disabled ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("aria-expanded", ItemContext.Open ? "true" : "false");

        // aria-controls is only emitted when the panel is open, matching Base UI's behaviour
        // where the attribute is set to undefined when closed.
        if (ItemContext.Open)
            yield return new("aria-controls", ItemContext.PanelId);

        if (ItemContext.Disabled)
            yield return new("disabled", true);

        yield return new("onclick",
            EventCallback.Factory.Create<MouseEventArgs>(this, async () =>
            {
                if (!ItemContext.Disabled)
                {
                    var wasOpen = ItemContext.Open;

                    // Close animations (including implicit closes from single-
                    // selection mode) are handled centrally by ToggleItemAsync.
                    await Context.Toggle(ItemContext.Value);

                    // Animate panel open via JS after state has been updated.
                    if (!wasOpen && Context.JsModule is not null)
                    {
                        try
                        {
                            await Context.JsModule.InvokeVoidAsync("openPanel", ItemContext.PanelId);
                        }
                        catch (JSDisconnectedException) { }
                        catch (OperationCanceledException) { }
                    }
                }
            }));
    }
}

public readonly record struct AccordionTriggerState(bool Open, bool Disabled, Orientation Orientation, int Index);
