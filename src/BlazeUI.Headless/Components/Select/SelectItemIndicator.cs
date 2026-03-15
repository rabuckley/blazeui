using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Select;

/// <summary>
/// Indicates whether the parent <see cref="SelectItem"/> is selected.
/// Conditionally mounts rather than toggling CSS visibility, unless
/// <see cref="KeepMounted"/> is set.
/// </summary>
public class SelectItemIndicator : ComponentBase
{
    [CascadingParameter] internal SelectItemContext ItemContext { get; set; } = default!;

    /// <summary>
    /// When <c>true</c>, always renders even when the item is not selected.
    /// Useful when animating the indicator in/out.
    /// </summary>
    [Parameter] public bool KeepMounted { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? Class { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private bool IsSelected => ItemContext?.Selected ?? false;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!KeepMounted && !IsSelected) return;

        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "aria-hidden", "true");
        if (AdditionalAttributes is not null) builder.AddMultipleAttributes(2, AdditionalAttributes);
        if (!string.IsNullOrEmpty(Class)) builder.AddAttribute(3, "class", Class);

        // data-selected mirrors Base UI's SelectItemIndicatorDataAttributes.
        // The attribute is absent (not data-unchecked) when unselected.
        if (IsSelected) builder.AddAttribute(4, "data-selected", "");

        builder.AddContent(5, ChildContent);
        builder.CloseElement();
    }
}
