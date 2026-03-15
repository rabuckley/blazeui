using BlazeUI.Headless.Components.Combobox;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Autocomplete;

/// <summary>
/// Renders the current autocomplete input value. Doesn't render its own HTML element —
/// it renders the value directly inline, or delegates to a child for custom rendering.
/// Matches Base UI's <c>Autocomplete.Value</c>.
/// </summary>
/// <remarks>
/// Three rendering modes:
/// <list type="bullet">
///   <item>No children — renders the current input value string directly.</item>
///   <item><see cref="ChildContent"/> (static) — overrides the value display entirely with custom markup.</item>
///   <item><see cref="ValueContent"/> (function) — receives the current string value and renders arbitrary markup.</item>
/// </list>
/// </remarks>
public class AutocompleteValue : ComponentBase
{
    [CascadingParameter] internal ComboboxContext Context { get; set; } = default!;

    /// <summary>
    /// Static child content that overrides the value display entirely.
    /// When provided, <see cref="ValueContent"/> is ignored.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// A render function that receives the current string value and produces markup.
    /// Ignored when <see cref="ChildContent"/> is also provided.
    /// </summary>
    [Parameter] public RenderFragment<string>? ValueContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var value = Context.InputValue ?? "";

        if (ChildContent is not null)
            // Static children override the value display, e.g. a custom placeholder layout.
            builder.AddContent(0, ChildContent);
        else if (ValueContent is not null)
            // Function child — receives the current value so the consumer can format it.
            builder.AddContent(0, ValueContent, value);
        else
            builder.AddContent(0, value);
    }
}
