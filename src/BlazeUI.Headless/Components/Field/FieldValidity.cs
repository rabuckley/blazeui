using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Field;

/// <summary>
/// Render-prop component that exposes the field's validity state to its child content.
/// Use this to conditionally render markup based on whether the field is valid, touched, etc.
/// </summary>
public class FieldValidity : ComponentBase
{
    [CascadingParameter]
    internal FieldContext? Context { get; set; }

    /// <summary>
    /// A function that receives the current <see cref="FieldValidityState"/> and returns
    /// the content to render.
    /// </summary>
    [Parameter]
    public RenderFragment<FieldValidityState>? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (ChildContent is null || Context is null) return;

        var state = new FieldValidityState(
            Valid: !Context.Invalid,
            Invalid: Context.Invalid,
            Dirty: Context.Dirty,
            Touched: Context.Touched,
            Disabled: Context.Disabled,
            Focused: Context.Focused,
            Filled: Context.Filled
        );

        builder.AddContent(0, ChildContent(state));
    }
}

/// <summary>
/// Snapshot of field validity and interaction state provided to the <see cref="FieldValidity"/>
/// child content function.
/// </summary>
public readonly record struct FieldValidityState(
    bool Valid,
    bool Invalid,
    bool Dirty,
    bool Touched,
    bool Disabled,
    bool Focused,
    bool Filled
);
