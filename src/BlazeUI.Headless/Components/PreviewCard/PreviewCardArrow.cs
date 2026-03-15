using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.PreviewCard;

/// <summary>
/// Decorative arrow element positioned against the preview card anchor.
/// Always <c>aria-hidden="true"</c> — it is purely presentational.
/// </summary>
public class PreviewCardArrow : BlazeElement<PreviewCardArrowState>
{
    [CascadingParameter]
    internal PreviewCardContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";

    protected override PreviewCardArrowState GetCurrentState() =>
        new(Context.Open, Context.Side, Context.Align);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-open", Context.Open ? "" : null);
        yield return new("data-side", Context.Side);
        yield return new("data-align", Context.Align);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("aria-hidden", "true");
    }
}

public readonly record struct PreviewCardArrowState(bool Open, string Side, string Align);
