using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Menu;

/// <summary>
/// Indicates whether the checkbox item is ticked.
/// Renders a <c>&lt;span&gt;</c> element.
/// </summary>
public class MenuCheckboxItemIndicator : BlazeElement<MenuCheckboxItemIndicatorState>
{
    // Reads checked/disabled/highlighted state from the enclosing MenuCheckboxItem via cascade.
    [CascadingParameter] internal MenuCheckboxItemContext? ItemContext { get; set; }

    /// <summary>
    /// When <c>false</c> (the default), the indicator is only rendered when the item is checked.
    /// Set to <c>true</c> to keep it mounted for CSS transition support.
    /// </summary>
    [Parameter] public bool KeepMounted { get; set; }

    private bool IsChecked => ItemContext?.Checked ?? false;
    private bool IsDisabled => ItemContext?.Disabled ?? false;

    protected override string DefaultTag => "span";
    protected override MenuCheckboxItemIndicatorState GetCurrentState() => new(IsChecked, IsDisabled);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        // Base UI's MenuCheckboxItemIndicatorDataAttributes: checked, unchecked, disabled.
        // Highlighted is NOT emitted on the indicator — only on the checkbox item itself.
        yield return new("data-checked", IsChecked ? "" : null);
        yield return new("data-unchecked", !IsChecked ? "" : null);
        yield return new("data-disabled", IsDisabled ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("aria-hidden", "true");
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!KeepMounted && !IsChecked) return;
        base.BuildRenderTree(builder);
    }
}

public readonly record struct MenuCheckboxItemIndicatorState(bool Checked, bool Disabled);
