using BlazeUI.Headless.Components.CheckboxGroup;
using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Checkbox;

public class CheckboxRoot : BlazeElement<CheckboxState>
{
    [Parameter]
    public bool? Checked { get; set; }

    [Parameter]
    public EventCallback<bool> CheckedChanged { get; set; }

    [Parameter]
    public bool DefaultChecked { get; set; }

    [Parameter]
    public bool Indeterminate { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool ReadOnly { get; set; }

    [Parameter]
    public bool Required { get; set; }

    /// <summary>
    /// Form field name. Applied to the hidden checkbox input so the checkbox
    /// participates in native form submission.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// String value submitted when checked. Also used for CheckboxGroup integration,
    /// where the group manages checked state and this value identifies the checkbox.
    /// </summary>
    [Parameter]
    public string? Value { get; set; }

    [CascadingParameter]
    internal CheckboxGroupContext? GroupContext { get; set; }

    private readonly ComponentState<bool> _checked;

    public CheckboxRoot()
    {
        _checked = new ComponentState<bool>(false);
    }

    protected override void OnInitialized()
    {
        _checked.SetInternal(DefaultChecked);
    }

    protected override void OnParametersSet()
    {
        if (GroupContext is not null && Value is not null)
        {
            _checked.SetControlled(GroupContext.IsChecked(Value));
        }
        else if (Checked.HasValue)
        {
            _checked.SetControlled(Checked.Value);
        }
        else
        {
            _checked.ClearControlled();
        }
    }

    protected override string DefaultTag => "span";

    // Group disabled state takes precedence over the individual checkbox's Disabled prop,
    // matching Base UI's behavior where CheckboxGroup.disabled overrides child props.
    private bool EffectivelyDisabled => (GroupContext?.Disabled ?? false) || Disabled;

    protected override CheckboxState GetCurrentState() => new(_checked.Value, EffectivelyDisabled, Indeterminate, ReadOnly, Required);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-checked", _checked.Value ? "" : null);
        yield return new("data-unchecked", !_checked.Value && !Indeterminate ? "" : null);
        yield return new("data-indeterminate", Indeterminate ? "" : null);
        yield return new("data-disabled", EffectivelyDisabled ? "" : null);
        yield return new("data-readonly", ReadOnly ? "" : null);
        yield return new("data-required", Required ? "" : null);
    }

    private async Task HandleClick()
    {
        if (EffectivelyDisabled || ReadOnly) return;

        if (GroupContext is not null && Value is not null)
        {
            await GroupContext.Toggle(Value);
            return;
        }

        var newValue = !_checked.Value;

        // Update state FIRST, then notify
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

        // Keyboard focusability — <span> is not natively focusable like <button>.
        builder.AddAttribute(6, "tabindex", EffectivelyDisabled ? -1 : 0);

        // ARIA
        builder.AddAttribute(7, "role", "checkbox");
        var ariaChecked = Indeterminate ? "mixed" : (_checked.Value ? "true" : "false");
        builder.AddAttribute(8, "aria-checked", ariaChecked);
        if (EffectivelyDisabled)
            builder.AddAttribute(9, "aria-disabled", "true");
        if (ReadOnly)
            builder.AddAttribute(10, "aria-readonly", "true");
        if (Required)
            builder.AddAttribute(11, "aria-required", "true");

        builder.AddAttribute(12, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));

        // Cascade context to Indicator
        builder.OpenComponent<CascadingValue<CheckboxContext>>(12);
        builder.AddComponentParameter(13, "Value", new CheckboxContext
        {
            Checked = _checked.Value,
            Disabled = EffectivelyDisabled,
            Indeterminate = Indeterminate,
            ReadOnly = ReadOnly,
            Required = Required
        });
        builder.AddComponentParameter(14, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();

        // Hidden native checkbox input for form participation, matching Base UI's structure.
        // The user-provided id goes here so <label for="..."> clicks toggle the checkbox.
        builder.OpenElement(15, "input");
        if (inputId is not null)
            builder.AddAttribute(16, "id", inputId);
        if (Name is not null)
            builder.AddAttribute(17, "name", Name);
        if (EffectivelyDisabled)
            builder.AddAttribute(18, "disabled");
        builder.AddAttribute(19, "tabindex", "-1");
        builder.AddAttribute(20, "aria-hidden", "true");
        builder.AddAttribute(21, "type", "checkbox");
        if (Value is not null)
            builder.AddAttribute(22, "value", Value);
        if (_checked.Value)
            builder.AddAttribute(23, "checked");
        builder.AddAttribute(24, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));
        builder.AddAttribute(25, "style",
            "clip-path:inset(50%);overflow:hidden;white-space:nowrap;border:0;padding:0;width:1px;height:1px;margin:-1px;position:fixed;top:0;left:0");
        builder.CloseElement();
    }
}

public readonly record struct CheckboxState(bool Checked, bool Disabled, bool Indeterminate, bool ReadOnly, bool Required);
