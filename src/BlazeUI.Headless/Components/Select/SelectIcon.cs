using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Select;

/// <summary>
/// An icon that indicates that the trigger button opens a select popup.
/// Renders a <c>&lt;span&gt;</c> element.
/// </summary>
public class SelectIcon : BlazeElement<SelectIconState>
{
    [CascadingParameter] internal SelectContext Context { get; set; } = default!;

    protected override string DefaultTag => "span";

    protected override SelectIconState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-popup-open", Context.Open ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("aria-hidden", "true");
    }
}

public readonly record struct SelectIconState(bool Open);
