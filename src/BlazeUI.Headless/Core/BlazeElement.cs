using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Core;

/// <summary>
/// Base class for all BlazeUI headless components. Renders a single HTML element
/// with merged attributes, data-attribute state, and polymorphic tag support.
/// </summary>
/// <typeparam name="TState">
/// A type representing the component's current state, used by <see cref="ClassBuilder"/>
/// and <see cref="StyleBuilder"/> to produce dynamic class/style strings.
/// </typeparam>
public abstract class BlazeElement<TState> : ComponentBase
{
    /// <summary>
    /// Explicit HTML element ID. When <c>null</c>, an auto-generated ID is used.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Overrides the default HTML tag. For example, render a button as an anchor.
    /// Silently ignored when <see cref="Render"/> is provided.
    /// </summary>
    [Parameter]
    public string? As { get; set; }

    /// <summary>
    /// Content to render inside the element.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Static CSS class names to apply.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// A function that produces dynamic CSS classes based on the current component state.
    /// </summary>
    [Parameter]
    public Func<TState, string>? ClassBuilder { get; set; }

    /// <summary>
    /// Static inline styles to apply.
    /// </summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>
    /// A function that produces dynamic inline styles based on the current component state.
    /// </summary>
    [Parameter]
    public Func<TState, string>? StyleBuilder { get; set; }

    /// <summary>
    /// Replaces the default element with a custom render fragment. The fragment receives
    /// an <see cref="ElementProps"/> containing the merged attribute dictionary and child content,
    /// so the consumer can render any element while retaining all headless behavior via <c>@attributes</c>.
    /// </summary>
    [Parameter]
    public RenderFragment<ElementProps>? Render { get; set; }

    /// <summary>
    /// Additional HTML attributes to splat onto the rendered element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// The default HTML tag when <see cref="As"/> is not specified.
    /// </summary>
    protected abstract string DefaultTag { get; }

    /// <summary>
    /// Returns the current component state for use by builders and data attributes.
    /// </summary>
    protected abstract TState GetCurrentState();

    /// <summary>
    /// Returns data attributes representing the current component state.
    /// Null values are omitted from the rendered output.
    /// </summary>
    protected abstract IEnumerable<KeyValuePair<string, object?>> GetDataAttributes();

    private string? _resolvedId;

    /// <summary>
    /// The effective element ID — either the explicit <see cref="Id"/> or an auto-generated value.
    /// </summary>
    protected string ResolvedId => _resolvedId ??= Id ?? IdGenerator.Next(DefaultTag);

    /// <summary>
    /// The element ID to render. Override in triggers that use a context-provided ID
    /// (e.g. <c>Context.TriggerId</c>) instead of the default <see cref="ResolvedId"/>.
    /// </summary>
    protected virtual string ElementId => ResolvedId;

    /// <summary>
    /// Returns additional attributes (aria, event handlers) to merge into the rendered element.
    /// Override in triggers to inject behavior without duplicating the full render pipeline.
    /// These have the highest precedence and will override any consumer-supplied attributes.
    /// </summary>
    protected virtual IEnumerable<KeyValuePair<string, object>> GetExtraAttributes() => [];

    /// <summary>
    /// Builds the complete attribute dictionary with this precedence (last write wins):
    /// AdditionalAttributes → id → class → style → data-* → extra attributes.
    /// </summary>
    protected Dictionary<string, object> BuildAttributes(TState state)
    {
        var attrs = new Dictionary<string, object>();

        // HTML buttons default to type="submit", which accidentally submits forms.
        // Base UI's useButton hook sets type="button" as the safe default; we do
        // the same here. Consumers can override via additional attributes.
        var tag = As ?? DefaultTag;
        if (tag is "button")
            attrs["type"] = "button";

        // Consumer overrides have lowest precedence.
        if (AdditionalAttributes is not null)
        {
            foreach (var kvp in AdditionalAttributes)
            {
                if (kvp.Value is not null)
                    attrs[kvp.Key] = kvp.Value;
            }
        }

        attrs["id"] = ElementId;

        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass))
            attrs["class"] = mergedClass;

        var mergedStyle = Css.Cn(Style, StyleBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedStyle))
            attrs["style"] = mergedStyle;

        foreach (var attr in GetDataAttributes())
        {
            if (attr.Value is not null)
                attrs[attr.Key] = attr.Value;
        }

        // Component-specific attributes (aria, events) have highest precedence.
        foreach (var kvp in GetExtraAttributes())
            attrs[kvp.Key] = kvp.Value;

        return attrs;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var attrs = BuildAttributes(state);

        if (Render is not null)
        {
            builder.AddContent(0, Render, new ElementProps(attrs, ChildContent));
        }
        else
        {
            var tag = As ?? DefaultTag;
            builder.OpenElement(0, tag);
            builder.AddMultipleAttributes(1, attrs);
            builder.AddContent(2, ChildContent);
            builder.CloseElement();
        }
    }

}
