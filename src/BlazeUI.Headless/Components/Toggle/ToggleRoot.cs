using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Toggle;

public class ToggleRoot : BlazeElement<ToggleState>
{
    [Parameter]
    public bool? Pressed { get; set; }

    [Parameter]
    public EventCallback<bool> PressedChanged { get; set; }

    [Parameter]
    public bool DefaultPressed { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// String value for ToggleGroup integration. When inside a group, the group
    /// manages pressed state and this value identifies the toggle.
    /// </summary>
    [Parameter]
    public string? Value { get; set; }

    [CascadingParameter]
    internal ToggleGroupContext? GroupContext { get; set; }

    private readonly ComponentState<bool> _pressed;

    // The effective disabled state combines the toggle's own Disabled prop with
    // the group's Disabled prop, matching Base UI's behaviour.
    private bool EffectiveDisabled => Disabled || (GroupContext?.Disabled ?? false);

    public ToggleRoot()
    {
        _pressed = new ComponentState<bool>(false);
    }

    protected override void OnParametersSet()
    {
        // If inside a group, the group owns the pressed state.
        if (GroupContext is not null && Value is not null)
        {
            _pressed.SetControlled(GroupContext.IsPressed(Value));
        }
        else if (Pressed.HasValue)
        {
            _pressed.SetControlled(Pressed.Value);
        }
        else
        {
            _pressed.ClearControlled();
        }
    }

    protected override void OnInitialized()
    {
        _pressed.SetInternal(DefaultPressed);
    }

    protected override string DefaultTag => "button";

    protected override ToggleState GetCurrentState() => new(_pressed.Value, EffectiveDisabled);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-pressed", _pressed.Value ? "" : null);
        yield return new("data-disabled", EffectiveDisabled ? "" : null);
    }

    private async Task HandleClick()
    {
        if (EffectiveDisabled) return;

        var newValue = !_pressed.Value;

        if (GroupContext is not null && Value is not null)
        {
            await GroupContext.Toggle(Value);
            return;
        }

        // Update state FIRST, then notify.
        _pressed.SetInternal(newValue);
        if (PressedChanged.HasDelegate)
            await InvokeAsync(() => PressedChanged.InvokeAsync(newValue));
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddAttribute(1, "id", ResolvedId);

        if (AdditionalAttributes is not null)
            builder.AddMultipleAttributes(2, AdditionalAttributes);

        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass))
            builder.AddAttribute(3, "class", mergedClass);

        var mergedStyle = Css.Cn(Style, StyleBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedStyle))
            builder.AddAttribute(4, "style", mergedStyle);

        foreach (var attr in GetDataAttributes())
            if (attr.Value is not null)
                builder.AddAttribute(5, attr.Key, attr.Value);

        builder.AddAttribute(6, "aria-pressed", _pressed.Value ? "true" : "false");

        if (EffectiveDisabled)
            builder.AddAttribute(7, "disabled", true);

        builder.AddAttribute(8, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));

        builder.AddContent(9, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct ToggleState(bool Pressed, bool Disabled);

// Context class for ToggleGroup integration - defined here so ToggleRoot can reference it.
// The actual ToggleGroupRoot will cascade an instance of this.
internal class ToggleGroupContext
{
    private readonly Func<string, bool> _isPressed;
    private readonly Func<string, Task> _toggle;

    public ToggleGroupContext(Func<string, bool> isPressed, Func<string, Task> toggle, bool disabled)
    {
        _isPressed = isPressed;
        _toggle = toggle;
        Disabled = disabled;
    }

    public bool IsPressed(string value) => _isPressed(value);
    public Task Toggle(string value) => _toggle(value);
    public bool Disabled { get; }
}
