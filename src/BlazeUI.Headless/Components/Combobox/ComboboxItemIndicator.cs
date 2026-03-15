using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Combobox;

/// <summary>
/// Renders its children only when the parent <see cref="ComboboxItem"/> is selected.
/// Matches Base UI's <c>Combobox.ItemIndicator</c> which conditionally mounts
/// rather than toggling CSS visibility.
/// </summary>
public class ComboboxItemIndicator : ComponentBase
{
    // Preferred source: cascaded from ComboboxItem — no Value parameter required.
    [CascadingParameter] internal ComboboxItemContext? ItemContext { get; set; }

    // Fallback: root context used when an explicit Value is supplied.
    [CascadingParameter] internal ComboboxContext? Context { get; set; }

    /// <summary>
    /// The value of the parent item to check selection against. Only required when
    /// this indicator is rendered outside a <see cref="ComboboxItem"/>. Prefer nesting
    /// inside the item instead — the selected state is then inferred automatically.
    /// </summary>
    [Parameter] public string? Value { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? Class { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private bool IsSelected =>
        // Item context is the preferred source — populated when nested inside ComboboxItem.
        ItemContext?.Selected ??
        // Fallback: resolve against the root context when Value is provided explicitly.
        (Context?.IsValueSelected(Value ?? "") ?? false);

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!IsSelected) return;

        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "aria-hidden", "true");
        if (AdditionalAttributes is not null) builder.AddMultipleAttributes(2, AdditionalAttributes);
        if (!string.IsNullOrEmpty(Class)) builder.AddAttribute(3, "class", Class);
        builder.AddContent(4, ChildContent);
        builder.CloseElement();
    }
}
