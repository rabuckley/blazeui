using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Collapsible;

public class CollapsiblePanel : BlazeElement<CollapsiblePanelState>
{
    [CascadingParameter] internal CollapsibleContext Context { get; set; } = default!;

    /// <summary>
    /// When <c>true</c>, keeps the panel in the DOM when closed (sets the
    /// <c>hidden</c> attribute instead of unmounting). Defaults to <c>false</c>,
    /// which fully unmounts the panel on close — matching Base UI's default.
    /// </summary>
    [Parameter] public bool KeepMounted { get; set; }

    protected override string DefaultTag => "div";
    protected override CollapsiblePanelState GetCurrentState() => new(Context.Open);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-open", Context.Open ? "" : null);
        yield return new("data-closed", Context.Open ? null : "");
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Default behaviour (KeepMounted=false): fully unmount the panel when closed.
        // This matches Base UI's default where panel content is removed from the DOM.
        if (!KeepMounted && !Context.Open)
            return;

        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddAttribute(1, "id", Context.PanelId);
        if (AdditionalAttributes is not null) builder.AddMultipleAttributes(2, AdditionalAttributes);
        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass)) builder.AddAttribute(3, "class", mergedClass);
        var mergedStyle = Css.Cn(Style, StyleBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedStyle)) builder.AddAttribute(4, "style", mergedStyle);
        foreach (var attr in GetDataAttributes())
            if (attr.Value is not null) builder.AddAttribute(5, attr.Key, attr.Value);

        // When KeepMounted is true, hide the closed panel with the HTML hidden
        // attribute rather than removing it from the DOM.
        if (KeepMounted && !Context.Open)
            builder.AddAttribute(8, "hidden", true);

        builder.AddContent(9, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct CollapsiblePanelState(bool Open);
