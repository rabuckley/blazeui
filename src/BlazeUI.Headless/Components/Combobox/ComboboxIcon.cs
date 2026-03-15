using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Combobox;

/// <summary>
/// An icon that indicates the trigger button opens the combobox popup.
/// Renders a <c>&lt;span&gt;</c> element with <c>aria-hidden="true"</c> and
/// a default <c>▼</c> character. Matches Base UI's <c>Combobox.Icon</c>.
/// </summary>
public class ComboboxIcon : BlazeElement<ComboboxIconState>
{
    protected override string DefaultTag => "span";
    protected override ComboboxIconState GetCurrentState() => default;
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes() { yield break; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        if (AdditionalAttributes is not null) builder.AddMultipleAttributes(1, AdditionalAttributes);
        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass)) builder.AddAttribute(2, "class", mergedClass);
        builder.AddAttribute(3, "aria-hidden", "true");

        // Render children when supplied, otherwise fall back to the default chevron glyph.
        if (ChildContent is not null)
            builder.AddContent(4, ChildContent);
        else
            builder.AddContent(4, "▼");

        builder.CloseElement();
    }
}

public readonly record struct ComboboxIconState;
