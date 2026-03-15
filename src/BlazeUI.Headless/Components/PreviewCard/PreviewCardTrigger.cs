using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.PreviewCard;

public class PreviewCardTrigger : BlazeElement<PreviewCardTriggerState>
{
    [CascadingParameter]
    internal PreviewCardContext Context { get; set; } = default!;

    protected override string DefaultTag => "a";
    protected override string ElementId => Context.TriggerId;

    protected override PreviewCardTriggerState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-popup-open", Context.Open ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        if (Context.TitleId is not null)
            yield return new("aria-labelledby", Context.TitleId);
    }
}

public readonly record struct PreviewCardTriggerState(bool Open);
