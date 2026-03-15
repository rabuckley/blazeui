using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Fieldset;

public class FieldsetRoot : BlazeElement<FieldsetState>
{
    [Parameter]
    public bool Disabled { get; set; }

    private FieldsetContext? _context;

    protected override string DefaultTag => "fieldset";

    protected override FieldsetState GetCurrentState() => new(Disabled);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-disabled", Disabled ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        if (_context?.LegendId is { } legendId)
            yield return new("aria-labelledby", legendId);
        if (Disabled)
            yield return new("disabled", true);
    }

    protected override void OnInitialized()
    {
        _context = new FieldsetContext(() => InvokeAsync(StateHasChanged));
    }

    protected override void OnParametersSet()
    {
        if (_context is not null)
            _context.Disabled = Disabled;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var attrs = BuildAttributes(state);

        var tag = As ?? DefaultTag;

        if (Render is not null)
        {
            // Cascade context before invoking the custom render fragment.
            builder.OpenComponent<CascadingValue<FieldsetContext>>(0);
            builder.AddComponentParameter(1, "Value", _context);
            builder.AddComponentParameter(2, "ChildContent", (RenderFragment)(b =>
                b.AddContent(0, Render, new ElementProps(attrs, ChildContent))));
            builder.CloseComponent();
        }
        else
        {
            builder.OpenElement(0, tag);
            builder.AddMultipleAttributes(1, attrs);

            // Cascade context so FieldsetLegend can register its ID.
            builder.OpenComponent<CascadingValue<FieldsetContext>>(2);
            builder.AddComponentParameter(3, "Value", _context);
            builder.AddComponentParameter(4, "ChildContent", ChildContent);
            builder.CloseComponent();

            builder.CloseElement();
        }
    }
}

public readonly record struct FieldsetState(bool Disabled);
