using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.ContextMenu;

public class ContextMenuRadioItem : BlazeElement<ContextMenuRadioItemState>
{
    [CascadingParameter] internal ContextMenuContext Context { get; set; } = default!;

    [Parameter, EditorRequired] public string Value { get; set; } = "";
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool CloseOnClick { get; set; } = true;

    private bool IsChecked => Context.RadioGroupValue == Value;
    private bool IsHighlighted => Context.HighlightedItemId == ResolvedId;

    protected override string DefaultTag => "div";
    protected override ContextMenuRadioItemState GetCurrentState() => new(IsChecked, Disabled, IsHighlighted);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-checked", IsChecked ? "" : null);
        yield return new("data-unchecked", !IsChecked ? "" : null);
        yield return new("data-disabled", Disabled ? "" : null);
        yield return new("data-highlighted", IsHighlighted ? "" : null);
    }

    private async Task HandleClick()
    {
        if (Disabled) return;
        if (Context.OnRadioGroupChange is not null)
            await Context.OnRadioGroupChange(Value);
        if (CloseOnClick)
            await Context.Close();
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

        builder.AddAttribute(6, "role", "menuitemradio");
        builder.AddAttribute(7, "aria-checked", IsChecked ? "true" : "false");
        builder.AddAttribute(8, "tabindex", Disabled ? "-1" : "0");
        builder.AddAttribute(9, "onclick",
            EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));

        builder.AddContent(10, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct ContextMenuRadioItemState(bool Checked, bool Disabled, bool Highlighted);
