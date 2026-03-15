using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Menu;

public class MenuRadioItem : BlazeElement<MenuRadioItemState>
{
    [CascadingParameter] internal MenuContext Context { get; set; } = default!;
    [CascadingParameter] internal MenuRadioGroupContext? RadioGroupContext { get; set; }

    [Parameter, EditorRequired] public string Value { get; set; } = "";
    [Parameter] public bool Disabled { get; set; }

    /// <summary>Whether to close the menu after clicking this item. Matches Base UI's default of <c>false</c>.</summary>
    [Parameter] public bool CloseOnClick { get; set; }

    // Disabled state: the group's disabled flag takes precedence over the item's own flag.
    private bool IsDisabled => (RadioGroupContext?.Disabled ?? false) || Disabled;

    private bool IsChecked => RadioGroupContext?.Value == Value;
    private bool IsHighlighted => Context.HighlightedItemId == ResolvedId;

    private readonly MenuRadioItemContext _itemContext = new();

    protected override string DefaultTag => "div";
    protected override MenuRadioItemState GetCurrentState() => new(IsChecked, IsDisabled, IsHighlighted);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-checked", IsChecked ? "" : null);
        yield return new("data-unchecked", !IsChecked ? "" : null);
        yield return new("data-disabled", IsDisabled ? "" : null);
        yield return new("data-highlighted", IsHighlighted ? "" : null);
    }

    private async Task HandleClick()
    {
        if (IsDisabled) return;
        if (RadioGroupContext is not null)
            await RadioGroupContext.SetValue(Value);
        if (CloseOnClick)
            await Context.Close();
    }

    /// <summary>
    /// Close any open sibling submenu when the pointer enters this item.
    /// </summary>
    private Task HandlePointerEnter()
    {
        if (Context.CloseOpenSubmenu is not null)
            return Context.CloseOpenSubmenu();
        return Task.CompletedTask;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Push current state into the item context so MenuRadioItemIndicator can read it
        // without needing its own parameters.
        _itemContext.Checked = IsChecked;
        _itemContext.Disabled = IsDisabled;

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

        builder.AddAttribute(6, "role", "menuitemradio");
        builder.AddAttribute(7, "aria-checked", IsChecked ? "true" : "false");
        builder.AddAttribute(8, "tabindex", IsDisabled ? "-1" : "0");
        builder.AddAttribute(9, "onclick",
            EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));
        builder.AddAttribute(10, "onpointerenter",
            EventCallback.Factory.Create<PointerEventArgs>(this, HandlePointerEnter));

        // Cascade the item context so MenuRadioItemIndicator can read checked state.
        builder.OpenComponent<CascadingValue<MenuRadioItemContext>>(11);
        builder.AddComponentParameter(12, "Value", _itemContext);
        builder.AddComponentParameter(13, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();
    }
}

public readonly record struct MenuRadioItemState(bool Checked, bool Disabled, bool Highlighted);
