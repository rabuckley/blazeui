using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Dialog;

public class DialogClose : BlazeElement<DialogCloseState>
{
    [CascadingParameter]
    internal DialogContext Context { get; set; } = default!;

    /// <summary>Whether the close button is disabled. A disabled close button does not close the dialog.</summary>
    [Parameter] public bool Disabled { get; set; }

    protected override string DefaultTag => "button";

    protected override DialogCloseState GetCurrentState() => new(Disabled);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-disabled", Disabled ? "" : null);
    }

    private async Task HandleClick()
    {
        if (Disabled) return;
        await Context.Close();
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        if (Disabled)
            yield return new("disabled", true);

        yield return new("onclick",
            EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));
    }
}

public readonly record struct DialogCloseState(bool Disabled);
