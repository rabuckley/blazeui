using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Dialog;

public class DialogTitle : BlazeElement<DialogTitleState>
{
    [CascadingParameter]
    internal DialogContext Context { get; set; } = default!;

    protected override string DefaultTag => "h2";

    /// <summary>
    /// Use the Root's pre-generated TitleId so the popup's <c>aria-labelledby</c>
    /// is correct on the first Portal render. Falls back to <see cref="BlazeElement{TState}.ResolvedId"/>
    /// for standalone usage without a Root.
    /// </summary>
    protected override string ElementId => Id ?? Context.TitleId ?? ResolvedId;

    protected override DialogTitleState GetCurrentState() => default;

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield break;
    }

    protected override void OnInitialized()
    {
        Context.TitleId = ElementId;
    }
}

public readonly record struct DialogTitleState;
