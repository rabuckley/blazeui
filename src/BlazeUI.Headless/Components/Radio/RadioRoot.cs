using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Radio;

public class RadioRoot : BlazeElement<RadioState>
{
    [Parameter, EditorRequired]
    public string Value { get; set; } = "";

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool ReadOnly { get; set; }

    [Parameter]
    public bool Required { get; set; }

    [CascadingParameter]
    internal RadioGroupContext GroupContext { get; set; } = default!;

    private bool IsChecked => GroupContext.IsChecked(Value);

    // Effective property values merge local params with group-level overrides.
    private bool EffectiveDisabled => Disabled || GroupContext.Disabled;
    private bool EffectiveReadOnly => ReadOnly || GroupContext.ReadOnly;
    private bool EffectiveRequired => Required || GroupContext.Required;

    protected override string DefaultTag => "span";

    protected override RadioState GetCurrentState() =>
        new(IsChecked, EffectiveDisabled, EffectiveReadOnly, EffectiveRequired);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-checked", IsChecked ? "" : null);
        yield return new("data-unchecked", !IsChecked ? "" : null);
        yield return new("data-disabled", EffectiveDisabled ? "" : null);
        yield return new("data-readonly", EffectiveReadOnly ? "" : null);
        yield return new("data-required", EffectiveRequired ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("role", "radio");

        // Roving tabindex: only the checked item (or the first if none checked) is tabbable.
        yield return new("tabindex", IsChecked ? "0" : "-1");

        yield return new("aria-checked", IsChecked ? "true" : "false");

        // Only emit when true to match Base UI's conditional attribute pattern.
        if (EffectiveRequired)
            yield return new("aria-required", "true");
        if (EffectiveReadOnly)
            yield return new("aria-readonly", "true");
        if (EffectiveDisabled)
            yield return new("aria-disabled", "true");

        yield return new("onclick",
            EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));
    }

    private async Task HandleClick()
    {
        if (EffectiveDisabled || EffectiveReadOnly) return;
        await GroupContext.Select(Value);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var attrs = BuildAttributes(state);

        // Base UI routes the user-provided id to the hidden input (for <label for="...">
        // association) and uses an auto-generated id on the visible element.
        var inputId = Id;
        if (inputId is not null)
        {
            // Replace the id in attrs with an auto-generated one for the visible element.
            attrs["id"] = IdGenerator.Next(DefaultTag);
        }

        var tag = As ?? DefaultTag;
        builder.OpenElement(0, tag);
        builder.AddMultipleAttributes(1, attrs);

        // Cascade RadioRootContext so RadioIndicator can read per-radio state
        // without needing its own Value prop.
        builder.OpenComponent<CascadingValue<RadioRootContext>>(2);
        builder.AddComponentParameter(3, "Value", new RadioRootContext
        {
            Checked = IsChecked,
            Disabled = EffectiveDisabled,
            ReadOnly = EffectiveReadOnly,
            Required = EffectiveRequired
        });
        builder.AddComponentParameter(4, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();

        // Hidden native radio input for form participation, matching Base UI's structure.
        // The user-provided id goes here so <label for="..."> clicks select the radio.
        // The onclick handler enables label-click → hidden-input-click → selection.
        builder.OpenElement(5, "input");
        if (inputId is not null)
            builder.AddAttribute(6, "id", inputId);
        builder.AddAttribute(7, "tabindex", "-1");
        builder.AddAttribute(8, "aria-hidden", "true");
        builder.AddAttribute(9, "type", "radio");
        builder.AddAttribute(10, "value", Value);
        if (GroupContext.Name is not null)
            builder.AddAttribute(11, "name", GroupContext.Name);
        if (IsChecked)
            builder.AddAttribute(12, "checked");
        if (EffectiveDisabled)
            builder.AddAttribute(13, "disabled");
        if (EffectiveRequired)
            builder.AddAttribute(14, "required");
        if (EffectiveReadOnly)
            builder.AddAttribute(15, "readonly");
        builder.AddAttribute(16, "style",
            "clip-path:inset(50%);overflow:hidden;white-space:nowrap;border:0;padding:0;width:1px;height:1px;margin:-1px;position:fixed;top:0;left:0");
        builder.AddAttribute(17, "onclick",
            EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));
        builder.CloseElement();
    }
}

public readonly record struct RadioState(bool Checked, bool Disabled, bool ReadOnly, bool Required);
