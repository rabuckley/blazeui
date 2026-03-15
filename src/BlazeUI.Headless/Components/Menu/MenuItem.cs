using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Menu;

public class MenuItem : BlazeElement<MenuItemState>
{
    [CascadingParameter] internal MenuContext Context { get; set; } = default!;

    [Parameter] public bool Disabled { get; set; }
    [Parameter] public EventCallback OnClick { get; set; }

    /// <summary>Close the menu after clicking this item. Defaults to true.</summary>
    [Parameter] public bool CloseOnClick { get; set; } = true;

    private bool IsHighlighted => Context.HighlightedItemId == ResolvedId;

    protected override string DefaultTag => "div";
    protected override MenuItemState GetCurrentState() => new(IsHighlighted, Disabled);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-highlighted", IsHighlighted ? "" : null);
        yield return new("data-disabled", Disabled ? "" : null);
    }

    private async Task HandleClick()
    {
        if (Disabled) return;
        if (OnClick.HasDelegate)
            await OnClick.InvokeAsync();
        if (CloseOnClick)
            await Context.Close();
    }

    /// <summary>
    /// Close any open sibling submenu when the pointer enters this item.
    /// In Base UI, hovering any item in the parent menu closes open child submenus.
    /// </summary>
    private Task HandlePointerEnter()
    {
        if (Context.CloseOpenSubmenu is not null)
            return Context.CloseOpenSubmenu();
        return Task.CompletedTask;
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

        builder.AddAttribute(6, "role", "menuitem");
        builder.AddAttribute(7, "tabindex", Disabled ? "-1" : "0");
        if (Disabled) builder.AddAttribute(8, "aria-disabled", "true");
        builder.AddAttribute(9, "onclick",
            EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));
        builder.AddAttribute(10, "onpointerenter",
            EventCallback.Factory.Create<PointerEventArgs>(this, HandlePointerEnter));

        builder.AddContent(11, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct MenuItemState(bool Highlighted, bool Disabled);
