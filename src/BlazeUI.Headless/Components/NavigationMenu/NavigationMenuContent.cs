using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.NavigationMenu;

public class NavigationMenuContent : BlazeElement<NavigationMenuContentState>, IDisposable
{
    [CascadingParameter] internal NavigationMenuContext Context { get; set; } = default!;

    [Parameter, EditorRequired] public string Value { get; set; } = "";

    protected override string DefaultTag => "div";

    private bool IsOpen => Context.ActiveValue == Value;

    // Keep the previous item mounted while it animates out so CSS transitions
    // can complete. Once the animation finishes, the root calls StateHasChanged
    // to re-render and remove the previous item.
    private bool ShouldMountContent => IsOpen || Context.PreviousValue == Value;

    protected override NavigationMenuContentState GetCurrentState() => new(IsOpen);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-open", IsOpen ? "" : null);
        yield return new("data-closed", !IsOpen ? "" : null);
        // Set the activation direction on both entering and exiting content
        // so CSS can apply directional slide transitions in both directions.
        yield return new("data-activation-direction", ShouldMountContent ? Context.ActivationDirection : null);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Context.RegisterContentFragment(Value, BuildContentElement);
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        Context.RegisterContentFragment(Value, BuildContentElement);
    }

    /// <summary>
    /// Content no longer renders inline — it registers a <see cref="RenderFragment"/>
    /// with the context, and <see cref="NavigationMenuViewport"/> renders it inside
    /// the viewport element.
    /// </summary>
    protected override void BuildRenderTree(RenderTreeBuilder builder) { }

    /// <summary>
    /// Builds the actual content element (div with id, class, data attrs, ChildContent).
    /// This is the fragment registered with the context and rendered by the viewport.
    /// </summary>
    private void BuildContentElement(RenderTreeBuilder builder)
    {
        if (!ShouldMountContent) return;

        var state = GetCurrentState();
        var tag = As ?? DefaultTag;
        var contentId = Context.GetContentId(Value);

        builder.OpenElement(0, tag);
        if (contentId is not null) builder.AddAttribute(1, "id", contentId);
        if (AdditionalAttributes is not null) builder.AddMultipleAttributes(2, AdditionalAttributes);
        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass)) builder.AddAttribute(3, "class", mergedClass);
        var mergedStyle = Css.Cn(Style, StyleBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedStyle)) builder.AddAttribute(4, "style", mergedStyle);
        foreach (var attr in GetDataAttributes())
            if (attr.Value is not null) builder.AddAttribute(5, attr.Key, attr.Value);

        builder.AddContent(6, ChildContent);
        builder.CloseElement();
    }

    public void Dispose()
    {
        Context.UnregisterContentFragment(Value);
    }
}

public readonly record struct NavigationMenuContentState(bool Open);
