using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.ContextMenu;

public class ContextMenuRadioItemIndicator : BlazeElement<ContextMenuRadioItemIndicatorState>
{
    [CascadingParameter] internal ContextMenuContext Context { get; set; } = default!;

    /// <summary>Value to match against the radio group's selected value.</summary>
    [Parameter, EditorRequired] public string Value { get; set; } = "";
    [Parameter] public bool KeepMounted { get; set; }

    private bool IsChecked => Context.RadioGroupValue == Value;

    protected override string DefaultTag => "span";
    protected override ContextMenuRadioItemIndicatorState GetCurrentState() => new(IsChecked);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-checked", IsChecked ? "" : null);
        yield return new("data-unchecked", !IsChecked ? "" : null);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!KeepMounted && !IsChecked) return;
        base.BuildRenderTree(builder);
    }
}

public readonly record struct ContextMenuRadioItemIndicatorState(bool Checked);
