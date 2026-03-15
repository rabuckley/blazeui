using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Toast;

/// <summary>
/// Groups all parts of an individual toast. Emits ARIA attributes for the dialog
/// landmark role and wires title/description IDs for accessible naming.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
public class ToastRoot : BlazeElement<ToastRootState>
{
    /// <summary>Unique identifier for this toast instance.</summary>
    [Parameter] public string ToastId { get; set; } = "";

    /// <summary>
    /// Application-defined type string (e.g. "success", "error", "warning").
    /// Propagated to child sub-parts via <see cref="ToastContext"/> and emitted as
    /// <c>data-type</c> on the root and applicable children.
    /// </summary>
    [Parameter] public string? ToastType { get; set; }

    private readonly ToastContext _context;

    public ToastRoot()
    {
        _context = new ToastContext
        {
            NotifyChanged = () => StateHasChanged(),
        };
    }

    protected override string DefaultTag => "div";

    protected override void OnParametersSet()
    {
        _context.ToastId = ToastId;
        _context.ToastType = ToastType;
    }

    protected override ToastRootState GetCurrentState() => new(ToastType);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        if (ToastType is not null)
            yield return new("data-type", ToastType);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        // Base UI renders role="alertdialog" for high-priority toasts and role="dialog" for normal.
        // BlazeUI does not currently model toast priority, so always use role="dialog".
        // TODO: add a Priority parameter and switch to "alertdialog" when Priority == "high".
        yield return new("role", "dialog");
        yield return new("tabindex", "0");
        yield return new("aria-modal", "false");

        if (_context.TitleId is not null)
            yield return new("aria-labelledby", _context.TitleId);

        if (_context.DescriptionId is not null)
            yield return new("aria-describedby", _context.DescriptionId);
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

        var mergedStyle = Css.Cn(Style, StyleBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedStyle))
            builder.AddAttribute(4, "style", mergedStyle);

        foreach (var attr in GetDataAttributes())
            if (attr.Value is not null)
                builder.AddAttribute(5, attr.Key, attr.Value);

        foreach (var attr in GetExtraAttributes())
            builder.AddAttribute(6, attr.Key, attr.Value);

        // Cascade context so child sub-parts (title, description, close, action, content)
        // can read toast state and register their IDs for ARIA wiring.
        builder.OpenComponent<CascadingValue<ToastContext>>(7);
        builder.AddComponentParameter(8, "Value", _context);
        builder.AddComponentParameter(9, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();
    }
}

/// <summary>State exposed to <see cref="ToastRoot"/>'s class/style builders.</summary>
public readonly record struct ToastRootState(string? Type);
