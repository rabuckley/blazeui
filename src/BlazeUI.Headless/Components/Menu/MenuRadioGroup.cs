using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Menu;

/// <summary>
/// Groups related radio items.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
/// <remarks>
/// Cascades a <see cref="MenuRadioGroupContext"/> to child <see cref="MenuRadioItem"/> components
/// so they can read and update the selected value independently of the parent
/// <see cref="MenuContext"/>, allowing multiple radio groups within a single menu.
/// </remarks>
public class MenuRadioGroup : BlazeElement<MenuRadioGroupState>
{
    private readonly MenuRadioGroupContext _radioGroupContext;

    public MenuRadioGroup()
    {
        _radioGroupContext = new MenuRadioGroupContext();
    }

    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string> ValueChanged { get; set; }

    /// <summary>Whether the entire radio group (and all its items) is disabled.</summary>
    [Parameter] public bool Disabled { get; set; }

    protected override string DefaultTag => "div";
    protected override MenuRadioGroupState GetCurrentState() => new(Disabled);
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes() { yield break; }

    protected override void OnParametersSet()
    {
        // Keep context in sync with controlled value and disabled state from the parent.
        _radioGroupContext.Value = Value;
        _radioGroupContext.Disabled = Disabled;

        // Wire SetValue so radio items can update the group and invoke the callback.
        _radioGroupContext.SetValue = async newValue =>
        {
            _radioGroupContext.Value = newValue;
            if (ValueChanged.HasDelegate)
                await ValueChanged.InvokeAsync(newValue);
            StateHasChanged();
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
        // aria-disabled signals the disabled state to assistive technologies.
        if (Disabled) builder.AddAttribute(5, "aria-disabled", "true");

        // Cascade MenuRadioGroupContext so descendant MenuRadioItem components can read
        // the current value and update it when clicked.
        builder.OpenComponent<CascadingValue<MenuRadioGroupContext>>(6);
        builder.AddComponentParameter(7, "Value", _radioGroupContext);
        builder.AddComponentParameter(8, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();
    }
}

public readonly record struct MenuRadioGroupState(bool Disabled);
