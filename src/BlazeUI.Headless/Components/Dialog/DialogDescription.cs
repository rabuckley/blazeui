using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Dialog;

public class DialogDescription : BlazeElement<DialogDescriptionState>
{
    [CascadingParameter]
    internal DialogContext Context { get; set; } = default!;

    protected override string DefaultTag => "p";

    /// <summary>
    /// Use the Root's pre-generated DescriptionId so the popup's
    /// <c>aria-describedby</c> is correct on the first Portal render.
    /// Falls back to <see cref="BlazeElement{TState}.ResolvedId"/> for standalone usage.
    /// </summary>
    protected override string ElementId => Id ?? Context.DescriptionId ?? ResolvedId;

    protected override DialogDescriptionState GetCurrentState() => default;

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield break;
    }

    protected override void OnInitialized()
    {
        Context.DescriptionId = ElementId;
    }
}

public readonly record struct DialogDescriptionState;
