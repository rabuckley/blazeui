using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.InputOTP;

/// <summary>
/// Renders a single OTP slot. Reads its state from <see cref="InputOTPContext"/>
/// by index and exposes <c>data-active</c> for styling.
/// </summary>
public class InputOTPSlot : BlazeElement<InputOTPSlotState>
{
    [CascadingParameter] internal InputOTPContext Context { get; set; } = default!;

    [Parameter, EditorRequired] public int Index { get; set; }

    protected override string DefaultTag => "div";

    protected override InputOTPSlotState GetCurrentState()
    {
        if (Context?.Slots is null || Index < 0 || Index >= Context.Slots.Length)
            return default;
        return Context.Slots[Index];
    }

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        var state = GetCurrentState();
        yield return new("data-active", state.IsActive ? "true" : "false");
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddAttribute(1, "id", ResolvedId);
        if (AdditionalAttributes is not null)
            builder.AddMultipleAttributes(2, AdditionalAttributes);

        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass))
            builder.AddAttribute(3, "class", mergedClass);

        foreach (var attr in GetDataAttributes())
            if (attr.Value is not null)
                builder.AddAttribute(5, attr.Key, attr.Value);

        // Character content.
        if (state.Char is not null)
            builder.AddContent(6, state.Char.Value.ToString());

        // Expose ChildContent for the styled layer to render the fake caret.
        if (ChildContent is not null)
            builder.AddContent(7, ChildContent);

        builder.CloseElement();
    }
}
