using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Combobox;

/// <summary>
/// Groups related combobox items under a shared label.
/// Renders <c>role="group"</c> and wires <c>aria-labelledby</c> to the
/// nested <see cref="ComboboxGroupLabel"/> automatically.
/// </summary>
public class ComboboxGroup : BlazeElement<ComboboxGroupState>
{
    private readonly ComboboxGroupContext _groupContext;

    public ComboboxGroup()
    {
        _groupContext = new ComboboxGroupContext
        {
            OnLabelRegistered = StateHasChanged,
        };
    }

    protected override string DefaultTag => "div";
    protected override ComboboxGroupState GetCurrentState() => default;
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
        builder.AddAttribute(4, "role", "group");
        if (_groupContext.LabelId is not null)
            builder.AddAttribute(5, "aria-labelledby", _groupContext.LabelId);

        // Cascade the group context so ComboboxGroupLabel can register its ID.
        builder.OpenComponent<CascadingValue<ComboboxGroupContext>>(6);
        builder.AddComponentParameter(7, "Value", _groupContext);
        builder.AddComponentParameter(8, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();
    }
}

public readonly record struct ComboboxGroupState;
