using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Menu;

/// <summary>
/// A menu item that toggles a setting on or off.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
public class MenuCheckboxItem : BlazeElement<MenuCheckboxItemState>
{
    [CascadingParameter] internal MenuContext Context { get; set; } = default!;

    [Parameter] public bool Checked { get; set; }
    [Parameter] public EventCallback<bool> CheckedChanged { get; set; }
    [Parameter] public bool Disabled { get; set; }

    /// <summary>Whether to close the menu after clicking this item. Matches Base UI's default of <c>false</c>.</summary>
    [Parameter] public bool CloseOnClick { get; set; }

    private bool IsHighlighted => Context.HighlightedItemId == ResolvedId;

    private readonly MenuCheckboxItemContext _itemContext = new();

    protected override string DefaultTag => "div";
    protected override MenuCheckboxItemState GetCurrentState() => new(Checked, Disabled, IsHighlighted);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-checked", Checked ? "" : null);
        yield return new("data-unchecked", !Checked ? "" : null);
        yield return new("data-disabled", Disabled ? "" : null);
        yield return new("data-highlighted", IsHighlighted ? "" : null);
    }

    private async Task HandleClick()
    {
        if (Disabled) return;
        if (CheckedChanged.HasDelegate)
            await CheckedChanged.InvokeAsync(!Checked);
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
        // Push current state into the item context so MenuCheckboxItemIndicator can read it
        // without needing its own parameters.
        _itemContext.Checked = Checked;
        _itemContext.Disabled = Disabled;

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

        builder.AddAttribute(6, "role", "menuitemcheckbox");
        builder.AddAttribute(7, "aria-checked", Checked ? "true" : "false");
        builder.AddAttribute(8, "tabindex", Disabled ? "-1" : "0");
        builder.AddAttribute(9, "onclick",
            EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));
        builder.AddAttribute(10, "onpointerenter",
            EventCallback.Factory.Create<PointerEventArgs>(this, HandlePointerEnter));

        // Cascade item context so the indicator can read checked/disabled state.
        builder.OpenComponent<CascadingValue<MenuCheckboxItemContext>>(11);
        builder.AddComponentParameter(12, "Value", _itemContext);
        builder.AddComponentParameter(13, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();
    }
}

public readonly record struct MenuCheckboxItemState(bool Checked, bool Disabled, bool Highlighted);
