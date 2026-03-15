using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.ContextMenu;

public class ContextMenuRadioGroup : BlazeElement<ContextMenuRadioGroupState>
{
    [CascadingParameter] internal ContextMenuContext ParentContext { get; set; } = default!;

    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string> ValueChanged { get; set; }

    protected override string DefaultTag => "div";
    protected override ContextMenuRadioGroupState GetCurrentState() => default;
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes() { yield break; }

    protected override void OnParametersSet()
    {
        ParentContext.RadioGroupValue = Value;
        ParentContext.OnRadioGroupChange = async (value) =>
        {
            if (ValueChanged.HasDelegate)
                await ValueChanged.InvokeAsync(value);
        };
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddAttribute(1, "id", ResolvedId);
        if (AdditionalAttributes is not null) builder.AddMultipleAttributes(2, AdditionalAttributes);
        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass)) builder.AddAttribute(3, "class", mergedClass);
        builder.AddAttribute(4, "role", "group");
        builder.AddContent(5, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct ContextMenuRadioGroupState;
