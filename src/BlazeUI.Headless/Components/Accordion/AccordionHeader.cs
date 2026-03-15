using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Accordion;

public class AccordionHeader : BlazeElement<AccordionHeaderState>
{
    [CascadingParameter] internal AccordionItemContext ItemContext { get; set; } = default!;

    protected override string DefaultTag => "h3";
    protected override AccordionHeaderState GetCurrentState() => new(ItemContext.Open, ItemContext.Disabled, ItemContext.Index);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-open", ItemContext.Open ? "" : null);
        yield return new("data-disabled", ItemContext.Disabled ? "" : null);
        yield return new("data-index", ItemContext.Index.ToString());
    }
}

public readonly record struct AccordionHeaderState(bool Open, bool Disabled, int Index);
