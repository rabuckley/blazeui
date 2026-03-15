using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.NavigationMenu;

public class NavigationMenuViewport : BlazeElement<NavigationMenuViewportState>, IDisposable
{
    [CascadingParameter] internal NavigationMenuContext Context { get; set; } = default!;

    protected override string DefaultTag => "div";

    private bool IsOpen => Context.ActiveValue is not null;

    protected override NavigationMenuViewportState GetCurrentState() => new(IsOpen);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-open", IsOpen ? "" : null);
        yield return new("data-closed", IsOpen ? null : "");
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Context.OnContentChanged += OnContentChanged;
    }

    private void OnContentChanged() => InvokeAsync(StateHasChanged);

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddAttribute(1, "id", Context.ViewportId);
        if (AdditionalAttributes is not null) builder.AddMultipleAttributes(2, AdditionalAttributes);
        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass)) builder.AddAttribute(3, "class", mergedClass);
        var mergedStyle = Css.Cn(Style, StyleBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedStyle)) builder.AddAttribute(4, "style", mergedStyle);
        foreach (var attr in GetDataAttributes())
            if (attr.Value is not null) builder.AddAttribute(5, attr.Key, attr.Value);

        // Render the active content fragment (and previous for exit animation)
        // inside the viewport. This is the Blazor equivalent of React's createPortal.
        if (Context.ActiveValue is not null)
        {
            var activeFragment = Context.GetContentFragment(Context.ActiveValue);
            if (activeFragment is not null) builder.AddContent(6, activeFragment);
        }
        if (Context.PreviousValue is not null && Context.PreviousValue != Context.ActiveValue)
        {
            var previousFragment = Context.GetContentFragment(Context.PreviousValue);
            if (previousFragment is not null) builder.AddContent(7, previousFragment);
        }

        builder.AddContent(8, ChildContent);
        builder.CloseElement();
    }

    public void Dispose()
    {
        Context.OnContentChanged -= OnContentChanged;
    }
}

public readonly record struct NavigationMenuViewportState(bool Open);
