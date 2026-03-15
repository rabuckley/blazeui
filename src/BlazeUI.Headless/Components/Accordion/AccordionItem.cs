using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Accordion;

public class AccordionItem : BlazeElement<AccordionItemState>
{
    [CascadingParameter] internal AccordionContext Context { get; set; } = default!;

    /// <summary>
    /// Unique identifier for this item, used to track which items are open.
    /// Auto-generated when not provided.
    /// </summary>
    [Parameter] public string? Value { get; set; }

    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Callback fired when this item's panel opens or closes.
    /// </summary>
    [Parameter] public EventCallback<bool> OnOpenChange { get; set; }

    private readonly AccordionItemContext _itemContext = new();
    private string _resolvedValue = "";

    protected override string DefaultTag => "div";

    protected override void OnInitialized()
    {
        _itemContext.Index = Context.GetNextIndex();
        _itemContext.TriggerId = IdGenerator.Next("accordion-trigger");
        _itemContext.PanelId = IdGenerator.Next("accordion-panel");
    }

    protected override async Task OnParametersSetAsync()
    {
        // Resolve the item value — use the provided Value or fall back to a stable
        // generated ID based on trigger ID to avoid re-generation on each render.
        _resolvedValue = Value ?? _itemContext.TriggerId;

        var wasOpen = _itemContext.Open;
        _itemContext.Value = _resolvedValue;
        _itemContext.Open = Context.OpenItems.Contains(_resolvedValue);
        _itemContext.Disabled = Disabled || Context.Disabled;

        // Register this item's panel ID so the root can animate implicit closes
        // (e.g. when another item opens in single-selection mode).
        Context.PanelIdsByValue[_resolvedValue] = _itemContext.PanelId;

        // Fire OnOpenChange when the open state transitions.
        if (_itemContext.Open != wasOpen && OnOpenChange.HasDelegate)
            await OnOpenChange.InvokeAsync(_itemContext.Open);
    }

    protected override AccordionItemState GetCurrentState() =>
        new(_itemContext.Open, _itemContext.Disabled, Context.Orientation, _itemContext.Index);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-open", _itemContext.Open ? "" : null);
        yield return new("data-disabled", _itemContext.Disabled ? "" : null);
        yield return new("data-orientation", Context.Orientation is Orientation.Horizontal ? "horizontal" : "vertical");
        yield return new("data-index", _itemContext.Index.ToString());
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
        var mergedStyle = Css.Cn(Style, StyleBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedStyle)) builder.AddAttribute(4, "style", mergedStyle);
        foreach (var attr in GetDataAttributes())
            if (attr.Value is not null) builder.AddAttribute(5, attr.Key, attr.Value);

        builder.OpenComponent<CascadingValue<AccordionItemContext>>(6);
        builder.AddComponentParameter(7, "Value", _itemContext);
        builder.AddComponentParameter(8, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();
    }
}

public readonly record struct AccordionItemState(bool Open, bool Disabled, Orientation Orientation, int Index);
