using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Toolbar;

/// <summary>
/// Groups related toolbar items. Renders with <c>role="group"</c>.
/// The group's disabled state cascades to children via <see cref="ToolbarGroupContext"/>.
/// </summary>
public class ToolbarGroup : BlazeElement<ToolbarGroupState>
{
    [CascadingParameter] internal ToolbarContext Context { get; set; } = default!;

    [Parameter] public bool Disabled { get; set; }

    // The effective disabled state combines the toolbar root's disabled flag with this group's own.
    private bool EffectiveDisabled => Context.Disabled || Disabled;

    protected override string DefaultTag => "div";
    protected override ToolbarGroupState GetCurrentState() => new(EffectiveDisabled, Context.Orientation);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-disabled", EffectiveDisabled ? "" : null);
        yield return new("data-orientation", Context.Orientation is Orientation.Horizontal ? "horizontal" : "vertical");
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("role", "group");
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var attrs = BuildAttributes(state);
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddMultipleAttributes(1, attrs);

        // Cascade group context so children can inherit the effective disabled state.
        var groupContext = new ToolbarGroupContext { Disabled = EffectiveDisabled };
        builder.OpenComponent<CascadingValue<ToolbarGroupContext>>(2);
        builder.AddComponentParameter(3, "Value", groupContext);
        builder.AddComponentParameter(4, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();
    }
}

public readonly record struct ToolbarGroupState(bool Disabled, Orientation Orientation);
