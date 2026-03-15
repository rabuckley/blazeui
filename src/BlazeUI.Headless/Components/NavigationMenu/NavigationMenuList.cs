using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.NavigationMenu;

public class NavigationMenuList : BlazeElement<NavigationMenuListState>
{
    [CascadingParameter] internal NavigationMenuContext Context { get; set; } = default!;

    protected override string DefaultTag => "ul";
    protected override NavigationMenuListState GetCurrentState() => new(Context.Orientation);

    // Base UI explicitly sets aria-orientation to undefined on the list — orientation is
    // conveyed by the root <nav> element's data-orientation attribute instead.
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes() => [];

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddAttribute(1, "id", Context.ListId);
        if (AdditionalAttributes is not null) builder.AddMultipleAttributes(2, AdditionalAttributes);
        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass)) builder.AddAttribute(3, "class", mergedClass);
        var mergedStyle = Css.Cn(Style, StyleBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedStyle)) builder.AddAttribute(4, "style", mergedStyle);

        builder.AddContent(5, ChildContent);
        builder.CloseElement();
    }
}

public readonly record struct NavigationMenuListState(Orientation Orientation);
