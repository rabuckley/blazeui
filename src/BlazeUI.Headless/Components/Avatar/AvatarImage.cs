using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace BlazeUI.Headless.Components.Avatar;

/// <summary>
/// The avatar image element. Only mounted in the DOM while the image is loading
/// or has successfully loaded; hidden when no src is provided or when an error occurs.
/// Fires <see cref="OnLoadingStatusChange"/> when the loading status transitions.
/// Renders as an <c>&lt;img&gt;</c> by default.
/// </summary>
public class AvatarImage : BlazeElement<AvatarState>
{
    [Parameter]
    public string? Src { get; set; }

    [Parameter]
    public string? Alt { get; set; }

    /// <summary>Called when the image's loading status changes.</summary>
    [Parameter]
    public EventCallback<AvatarLoadingStatus> OnLoadingStatusChange { get; set; }

    [CascadingParameter]
    internal AvatarContext Context { get; set; } = default!;

    protected override string DefaultTag => "img";

    protected override AvatarState GetCurrentState() => new(Context.Status);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-status", Context.Status.ToString().ToLowerInvariant());
    }

    protected override async Task OnParametersSetAsync()
    {
        // When a src is provided and the image hasn't started loading yet, transition
        // to 'loading'. When src is removed or empty, Base UI's useImageLoadingStatus
        // immediately sets status to 'error' — mirror that here so the fallback shows.
        if (!string.IsNullOrEmpty(Src))
        {
            if (Context.Status is AvatarLoadingStatus.Idle)
            {
                await Context.SetStatusAsync(AvatarLoadingStatus.Loading);
                if (OnLoadingStatusChange.HasDelegate)
                    await OnLoadingStatusChange.InvokeAsync(AvatarLoadingStatus.Loading);
            }
        }
        else if (Context.Status is not AvatarLoadingStatus.Idle)
        {
            // src removed — reset to idle so the fallback renders
            await Context.SetStatusAsync(AvatarLoadingStatus.Idle);
            if (OnLoadingStatusChange.HasDelegate)
                await OnLoadingStatusChange.InvokeAsync(AvatarLoadingStatus.Idle);
        }
    }

    private async Task HandleLoad()
    {
        await Context.SetStatusAsync(AvatarLoadingStatus.Loaded);
        if (OnLoadingStatusChange.HasDelegate)
            await OnLoadingStatusChange.InvokeAsync(AvatarLoadingStatus.Loaded);
    }

    private async Task HandleError()
    {
        await Context.SetStatusAsync(AvatarLoadingStatus.Error);
        if (OnLoadingStatusChange.HasDelegate)
            await OnLoadingStatusChange.InvokeAsync(AvatarLoadingStatus.Error);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // The image element must only be present in the DOM while loading or loaded.
        // Removing it when idle or errored lets the fallback take its place without
        // needing CSS visibility tricks.
        if (Context.Status is not (AvatarLoadingStatus.Loading or AvatarLoadingStatus.Loaded))
            return;

        var state = GetCurrentState();
        var attrs = BuildAttributes(state);

        // Merge img-specific attributes with highest precedence (they are semantically
        // required rather than stylistic, so they should not be overridden by consumers).
        if (!string.IsNullOrEmpty(Src))
            attrs["src"] = Src;
        if (!string.IsNullOrEmpty(Alt))
            attrs["alt"] = Alt;

        attrs["onload"] = EventCallback.Factory.Create<ProgressEventArgs>(this, HandleLoad);
        attrs["onerror"] = EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.ErrorEventArgs>(this, HandleError);

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
