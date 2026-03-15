using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.CheckboxGroup;

/// <summary>
/// Provides a shared checked-state to a series of checkboxes.
/// Renders a <c>div</c> with <c>role="group"</c>.
///
/// Documentation: <see href="https://base-ui.com/react/components/checkbox-group">Base UI CheckboxGroup</see>
/// </summary>
public class CheckboxGroupRoot : BlazeElement<CheckboxGroupState>
{
    /// <summary>
    /// The controlled set of checked values. Use <see cref="DefaultValue"/> for
    /// uncontrolled usage.
    /// </summary>
    [Parameter]
    public IReadOnlyList<string>? Value { get; set; }

    /// <summary>
    /// Called when the selection changes. Receives the new set of checked values.
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyList<string>> ValueChanged { get; set; }

    /// <summary>
    /// Initial checked values for uncontrolled usage. Ignored once the component mounts.
    /// </summary>
    [Parameter]
    public IReadOnlyList<string>? DefaultValue { get; set; }

    /// <summary>
    /// All possible values in the group. Required when using a parent checkbox — the
    /// parent reads this list to derive its checked/indeterminate state and to
    /// select or deselect all child checkboxes at once.
    /// </summary>
    [Parameter]
    public IReadOnlyList<string>? AllValues { get; set; }

    /// <summary>
    /// Whether all checkboxes in the group are disabled. Takes precedence over
    /// individual checkbox <c>Disabled</c> props.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// ID of the element that labels this group. Wired to <c>aria-labelledby</c>.
    /// </summary>
    [Parameter]
    public string? LabelledBy { get; set; }

    private readonly ComponentState<IReadOnlyList<string>> _value;

    // A persistent context object whose properties are mutated each render so that
    // CascadingValue's reference equality check doesn't short-circuit child re-renders
    // when the value list changes.
    private readonly CheckboxGroupContext _context;

    public CheckboxGroupRoot()
    {
        _value = new ComponentState<IReadOnlyList<string>>(Array.Empty<string>());

        _context = new CheckboxGroupContext
        {
            IsChecked = value => _value.Value.Contains(value),
            Toggle = ToggleAsync
        };
    }

    protected override void OnInitialized()
    {
        if (DefaultValue is not null)
            _value.SetInternal(DefaultValue);
    }

    protected override void OnParametersSet()
    {
        if (Value is not null)
            _value.SetControlled(Value);
        else
            _value.ClearControlled();

        // Sync mutable context properties so children see the latest state.
        _context.Disabled = Disabled;
        _context.AllValues = AllValues;
    }

    protected override string DefaultTag => "div";

    protected override CheckboxGroupState GetCurrentState() => new(Disabled);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        // Base UI emits data-disabled only when disabled.
        yield return new("data-disabled", Disabled ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("role", "group");

        if (!string.IsNullOrEmpty(LabelledBy))
            yield return new("aria-labelledby", LabelledBy);
    }

    private async Task ToggleAsync(string value)
    {
        if (Disabled) return;

        var newValues = new List<string>(_value.Value);
        if (newValues.Contains(value))
            newValues.Remove(value);
        else
            newValues.Add(value);

        _value.SetInternal(newValues);

        if (ValueChanged.HasDelegate)
            await InvokeAsync(() => ValueChanged.InvokeAsync(newValues));
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var attrs = BuildAttributes(state);

        var tag = As ?? DefaultTag;
        builder.OpenElement(0, tag);
        builder.AddMultipleAttributes(1, attrs);

        // Cascade context to all CheckboxRoot children. The context object is
        // mutated in OnParametersSet rather than replaced, so CascadingValue's
        // reference equality check doesn't prevent children from re-rendering.
        builder.OpenComponent<CascadingValue<CheckboxGroupContext>>(2);
        builder.AddComponentParameter(3, "Value", _context);
        builder.AddComponentParameter(4, "IsFixed", false);
        builder.AddComponentParameter(5, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();
    }
}

/// <summary>
/// Snapshot of <see cref="CheckboxGroupRoot"/> state, passed to <c>ClassBuilder</c>
/// and <c>StyleBuilder</c> delegates.
/// </summary>
public readonly record struct CheckboxGroupState(bool Disabled);
