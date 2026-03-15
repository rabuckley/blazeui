using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Switch;

public class SwitchRoot : BlazeElement<SwitchState>
{
    [Parameter]
    public bool? Checked { get; set; }

    [Parameter]
    public EventCallback<bool> CheckedChanged { get; set; }

    [Parameter]
    public bool DefaultChecked { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool ReadOnly { get; set; }

    [Parameter]
    public bool Required { get; set; }

    /// <summary>
    /// Form field name. Applied to the hidden checkbox input, not the visible element.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// The value submitted by the hidden input when the switch is checked.
    /// Defaults to "on", matching native checkbox behavior.
    /// </summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// When set, a second hidden input with this value is submitted when the switch is unchecked.
    /// This mirrors the Base UI <c>uncheckedValue</c> prop for unambiguous form submissions.
    /// </summary>
    [Parameter]
    public string? UncheckedValue { get; set; }

    private readonly ComponentState<bool> _checked;

    public SwitchRoot()
    {
        _checked = new ComponentState<bool>(false);
    }

    protected override void OnInitialized()
    {
        _checked.SetInternal(DefaultChecked);
    }

    protected override void OnParametersSet()
    {
        if (Checked.HasValue)
            _checked.SetControlled(Checked.Value);
        else
            _checked.ClearControlled();
    }

    protected override string DefaultTag => "span";

    protected override SwitchState GetCurrentState() => new(_checked.Value, Disabled, ReadOnly, Required);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-checked", _checked.Value ? "" : null);
        yield return new("data-unchecked", !_checked.Value ? "" : null);
        yield return new("data-disabled", Disabled ? "" : null);
        yield return new("data-readonly", ReadOnly ? "" : null);
        yield return new("data-required", Required ? "" : null);
    }

    private async Task HandleClick()
    {
        if (Disabled || ReadOnly) return;

        var newValue = !_checked.Value;

        // Update state FIRST, then notify.
        _checked.SetInternal(newValue);
        if (CheckedChanged.HasDelegate)
            await InvokeAsync(() => CheckedChanged.InvokeAsync(newValue));
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        // Base UI routes the user-provided id to the hidden input (for <label for="...">
        // association) and uses an auto-generated id on the visible element.
        var inputId = Id;
        var visibleId = Id is not null
            ? IdGenerator.Next(DefaultTag)
            : ResolvedId;

        builder.OpenElement(0, tag);
        builder.AddAttribute(1, "id", visibleId);

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

        // ARIA
        builder.AddAttribute(6, "role", "switch");
        builder.AddAttribute(7, "tabindex", Disabled ? "-1" : "0");
        builder.AddAttribute(8, "aria-checked", _checked.Value ? "true" : "false");
        if (Disabled)
            builder.AddAttribute(9, "aria-disabled", "true");
        if (ReadOnly)
            builder.AddAttribute(10, "aria-readonly", "true");
        if (Required)
            builder.AddAttribute(11, "aria-required", "true");

        builder.AddAttribute(12, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));

        // Cascade context to Thumb.
        builder.OpenComponent<CascadingValue<SwitchContext>>(13);
        builder.AddComponentParameter(14, "Value", new SwitchContext
        {
            Checked = _checked.Value,
            Disabled = Disabled,
            ReadOnly = ReadOnly,
            Required = Required,
        });
        builder.AddComponentParameter(15, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();

        // Hidden native checkbox input for form participation, matching Base UI's structure.
        // The user-provided id goes here so <label for="..."> clicks toggle the switch.
        builder.OpenElement(16, "input");
        if (inputId is not null)
            builder.AddAttribute(17, "id", inputId);
        if (Name is not null)
            builder.AddAttribute(18, "name", Name);
        if (Disabled)
            builder.AddAttribute(19, "disabled");
        builder.AddAttribute(20, "tabindex", "-1");
        builder.AddAttribute(21, "aria-hidden", "true");
        builder.AddAttribute(22, "type", "checkbox");
        if (Value is not null)
            builder.AddAttribute(23, "value", Value);
        if (_checked.Value)
            builder.AddAttribute(24, "checked");
        builder.AddAttribute(25, "style",
            "clip-path:inset(50%);overflow:hidden;white-space:nowrap;border:0;padding:0;width:1px;height:1px;margin:-1px;position:fixed;top:0;left:0");
        builder.CloseElement();

        // When an unchecked value is specified, emit a second hidden text input that
        // submits when the checkbox is absent from the form data (i.e. unchecked).
        // This mirrors Base UI's uncheckedValue behavior for unambiguous form parsing.
        if (UncheckedValue is not null && !_checked.Value)
        {
            builder.OpenElement(26, "input");
            builder.AddAttribute(27, "type", "hidden");
            if (Name is not null)
                builder.AddAttribute(28, "name", Name);
            builder.AddAttribute(29, "value", UncheckedValue);
            builder.AddAttribute(30, "aria-hidden", "true");
            builder.CloseElement();
        }
    }
}

public readonly record struct SwitchState(bool Checked, bool Disabled, bool ReadOnly, bool Required);
