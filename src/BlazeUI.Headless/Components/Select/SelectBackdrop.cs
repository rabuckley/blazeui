using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Select;

/// <summary>
/// An overlay displayed beneath the select popup.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
public class SelectBackdrop : BlazeElement<SelectBackdropState>
{
    [CascadingParameter] internal SelectContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";

    protected override SelectBackdropState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-open", Context.Open ? "" : null);
        yield return new("data-closed", !Context.Open ? "" : null);
    }
}

public readonly record struct SelectBackdropState(bool Open);
