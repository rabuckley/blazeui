using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Avatar;

/// <summary>
/// The root container for an avatar. Cascades loading status to child
/// <see cref="AvatarImage"/> and <see cref="AvatarFallback"/> components.
/// Renders as a <c>&lt;span&gt;</c> by default.
/// </summary>
public class AvatarRoot : BlazeElement<AvatarState>
{
    private AvatarContext? _context;

    protected override string DefaultTag => "span";

    protected override AvatarState GetCurrentState() => new(_context?.Status ?? AvatarLoadingStatus.Idle);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        var status = _context?.Status ?? AvatarLoadingStatus.Idle;
        yield return new("data-status", status.ToString().ToLowerInvariant());
    }

    protected override void OnInitialized()
    {
        _context = new AvatarContext(() => InvokeAsync(StateHasChanged));
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var attrs = BuildAttributes(state);

        // Wrap children in CascadingValue to share loading status with
        // AvatarImage and AvatarFallback, then render the outer element
        // (or the Render delegate) around it.
        var cascadedContent = (RenderFragment)(b =>
        {
            b.OpenComponent<CascadingValue<AvatarContext>>(0);
            b.AddComponentParameter(1, "Value", _context);
            b.AddComponentParameter(2, "IsFixed", true);
            b.AddComponentParameter(3, "ChildContent", ChildContent);
            b.CloseComponent();
        });

        if (Render is not null)
        {
            builder.AddContent(0, Render, new ElementProps(attrs, cascadedContent));
        }
        else
        {
            var tag = As ?? DefaultTag;
            builder.OpenElement(0, tag);
            builder.AddMultipleAttributes(1, attrs);
            builder.AddContent(2, cascadedContent);
            builder.CloseElement();
        }
    }
}

public readonly record struct AvatarState(AvatarLoadingStatus Status);
