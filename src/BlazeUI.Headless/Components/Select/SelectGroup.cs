using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Select;

/// <summary>
/// Groups related select items with the corresponding label.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
public class SelectGroup : BlazeElement<SelectGroupState>
{
    // The context lets SelectGroupLabel register its generated id so the group
    // can wire aria-labelledby. SetLabelId triggers a re-render of this component
    // so the aria attribute is present on the second pass (after the label initializes).
    private readonly SelectGroupContext _groupContext;

    protected override string DefaultTag => "div";
    protected override SelectGroupState GetCurrentState() => default;
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes() { yield break; }

    public SelectGroup()
    {
        // Capture the context in a local to satisfy the nullability checker inside the lambda
        // (the field is non-null by the time the delegate runs, but the compiler can't prove it).
        var ctx = new SelectGroupContext();
        ctx.SetLabelId = labelId =>
        {
            ctx.LabelId = labelId;
            StateHasChanged();
        };
        _groupContext = ctx;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenComponent<CascadingValue<SelectGroupContext>>(0);
        builder.AddComponentParameter(1, "Value", _groupContext);
        builder.AddComponentParameter(2, "IsFixed", true);
        builder.AddComponentParameter(3, "ChildContent", (RenderFragment)(inner =>
        {
            inner.OpenElement(0, tag);
            inner.AddAttribute(1, "id", ResolvedId);
            if (AdditionalAttributes is not null) inner.AddMultipleAttributes(2, AdditionalAttributes);
            var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
            if (!string.IsNullOrEmpty(mergedClass)) inner.AddAttribute(3, "class", mergedClass);
            inner.AddAttribute(4, "role", "group");

            // Wire aria-labelledby to the label registered by SelectGroupLabel.
            // This is populated on the second render pass after the label initializes.
            if (_groupContext.LabelId is not null)
                inner.AddAttribute(5, "aria-labelledby", _groupContext.LabelId);

            inner.AddContent(6, ChildContent);
            inner.CloseElement();
        }));
        builder.CloseComponent();
    }
}

public readonly record struct SelectGroupState;
