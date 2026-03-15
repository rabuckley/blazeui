using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Dialog;

public class DialogTrigger : BlazeElement<DialogTriggerState>
{
    [CascadingParameter]
    internal DialogContext Context { get; set; } = default!;

    /// <summary>Whether the trigger is disabled. A disabled trigger cannot open the dialog.</summary>
    [Parameter] public bool Disabled { get; set; }

    protected override string DefaultTag => "button";

    protected override DialogTriggerState GetCurrentState() => new(Context.Open, Disabled);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-popup-open", Context.Open ? "" : null);
        yield return new("data-disabled", Disabled ? "" : null);
    }

    private async Task HandleClick()
    {
        if (Disabled) return;
        await Context.SetOpen(!Context.Open);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("aria-haspopup", "dialog");
        yield return new("aria-expanded", Context.Open ? "true" : "false");
        yield return new("aria-controls", Context.PopupId);

        if (Disabled)
            yield return new("disabled", true);

        yield return new("onclick",
            EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));
    }
}

public readonly record struct DialogTriggerState(bool Open, bool Disabled);
