using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Select;

/// <summary>
/// Displays an element positioned against the select popup anchor.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
public class SelectArrow : BlazeElement<SelectArrowState>
{
    [CascadingParameter] internal SelectContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";

    protected override SelectArrowState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-open", Context.Open ? "" : null);
        yield return new("data-closed", !Context.Open ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("aria-hidden", "true");
    }
}

public readonly record struct SelectArrowState(bool Open);
