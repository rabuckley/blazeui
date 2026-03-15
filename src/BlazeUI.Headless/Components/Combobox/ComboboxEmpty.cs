using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Combobox;

/// <summary>
/// Renders its children only when the combobox filter matches no items.
/// Announces changes to screen readers via <c>role="status"</c> and
/// <c>aria-live="polite"</c>.
/// </summary>
public class ComboboxEmpty : BlazeElement<ComboboxEmptyState>
{
    [CascadingParameter] internal ComboboxContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";
    protected override ComboboxEmptyState GetCurrentState() => new(Context.IsEmpty);
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes() { yield break; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddAttribute(1, "id", ResolvedId);
        if (AdditionalAttributes is not null) builder.AddMultipleAttributes(2, AdditionalAttributes);
        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass)) builder.AddAttribute(3, "class", mergedClass);
        var mergedStyle = Css.Cn(Style, StyleBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedStyle)) builder.AddAttribute(4, "style", mergedStyle);

        builder.AddAttribute(5, "role", "status");
        builder.AddAttribute(6, "aria-live", "polite");
        builder.AddAttribute(7, "aria-atomic", "true");

        // Only render children when no items match the filter.
        if (state.Empty)
            builder.AddContent(8, ChildContent);

        builder.CloseElement();
    }
}

/// <summary>Backwards compatibility alias.</summary>
[Obsolete("Use ComboboxEmpty instead. This alias will be removed in a future release.")]
public class ComboboxNoResults : ComboboxEmpty;

public readonly record struct ComboboxEmptyState(bool Empty);

/// <summary>Backwards compatibility alias.</summary>
[Obsolete("Use ComboboxEmptyState instead. This alias will be removed in a future release.")]
public readonly record struct ComboboxNoResultsState(bool Empty);
