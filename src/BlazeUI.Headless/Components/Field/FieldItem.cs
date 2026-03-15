using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Field;

/// <summary>
/// Groups an individual checkbox or radio item with its label and description inside a
/// <see cref="FieldRoot"/>. Renders a <c>&lt;div&gt;</c> and cascades a
/// <see cref="FieldItemContext"/> with its own <c>disabled</c> state so nested controls
/// can respect per-item disabling independently from the parent field's disabled state.
///
/// <para>
/// Note: the parent <see cref="FieldRoot"/>'s <c>Disabled</c> property takes precedence
/// over <see cref="FieldItem"/>'s own <c>Disabled</c>; both are combined so that
/// disabling the whole field disables all items.
/// </para>
/// </summary>
public class FieldItem : BlazeElement<FieldItemState>
{
    [CascadingParameter]
    internal FieldContext? FieldContext { get; set; }

    /// <summary>
    /// Whether this particular item's controls are disabled, independent of the parent
    /// field's disabled state.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    private FieldItemContext? _context;

    protected override string DefaultTag => "div";

    protected override void OnInitialized()
    {
        _context = new FieldItemContext();
    }

    protected override void OnParametersSet()
    {
        if (_context is null) return;

        // The parent field's disabled state takes precedence over the item's own.
        _context.Disabled = Disabled || (FieldContext?.Disabled ?? false);
    }

    protected override FieldItemState GetCurrentState() => new(
        _context?.Disabled ?? false
    );

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-disabled", (_context?.Disabled ?? false) ? "" : null);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var attrs = BuildAttributes(state);

        var tag = As ?? DefaultTag;
        builder.OpenElement(0, tag);
        builder.AddMultipleAttributes(1, attrs);

        // Cascade the item context so nested Checkbox.Root / Radio.Root components
        // can read the per-item disabled state.
        builder.OpenComponent<CascadingValue<FieldItemContext>>(2);
        builder.AddComponentParameter(3, "Value", _context);
        builder.AddComponentParameter(4, "IsFixed", false);
        builder.AddComponentParameter(5, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();
    }
}

/// <summary>
/// Snapshot of <see cref="FieldItem"/> state, passed to <c>ClassBuilder</c>
/// and <c>StyleBuilder</c> delegates.
/// </summary>
public readonly record struct FieldItemState(bool Disabled);
