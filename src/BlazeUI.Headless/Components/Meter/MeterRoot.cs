using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Meter;

/// <summary>
/// Groups all parts of the meter and provides the value for screen readers.
/// Renders a <c>&lt;div&gt;</c> element with <c>role="meter"</c> and the
/// full ARIA value contract (<c>aria-valuenow</c>, <c>aria-valuemin</c>,
/// <c>aria-valuemax</c>, <c>aria-valuetext</c>).
/// </summary>
public class MeterRoot : BlazeElement<MeterState>
{
    /// <summary>The current numeric value of the meter.</summary>
    [Parameter]
    public double Value { get; set; }

    /// <summary>The minimum value. Defaults to <c>0</c>.</summary>
    [Parameter]
    public double Min { get; set; }

    /// <summary>The maximum value. Defaults to <c>100</c>.</summary>
    [Parameter]
    public double Max { get; set; } = 100;

    /// <summary>
    /// A function that returns a human-readable string for <c>aria-valuetext</c>,
    /// given the formatted display value and the raw numeric value.
    /// When omitted and no <see cref="Format"/> is supplied, defaults to <c>"{value}%"</c>.
    /// When omitted but <see cref="Format"/> is provided, defaults to the formatted value.
    /// </summary>
    [Parameter]
    public Func<string, double, string>? GetAriaValueText { get; set; }

    /// <summary>
    /// Options for formatting the value displayed in <see cref="MeterValue"/> and
    /// used as the default <c>aria-valuetext</c>.
    /// When <c>null</c>, the value is formatted as a percentage (e.g. <c>30%</c>).
    /// TODO: accept an <c>Intl.NumberFormat</c>-compatible options object for richer formatting.
    /// </summary>
    [Parameter]
    public string? Format { get; set; }

    private MeterContext? _context;

    protected override string DefaultTag => "div";

    protected override void OnInitialized()
    {
        _context = new MeterContext(() => InvokeAsync(StateHasChanged));
    }

    protected override void OnParametersSet()
    {
        if (_context is null) return;

        _context.Value = Value;
        _context.Min = Min;
        _context.Max = Max;
        _context.FormattedValue = ComputeFormattedValue();
    }

    // Mirrors Base UI's formatNumberValue: no format option → percent style (value / 100),
    // otherwise formats the raw value.
    private string ComputeFormattedValue()
    {
        if (string.IsNullOrEmpty(Format))
            return (Value / 100.0).ToString("P0").Replace(" ", " ");

        // TODO: parse Format string as Intl.NumberFormat options for richer formatting.
        return Value.ToString();
    }

    private string ComputeAriaValueText()
    {
        var formattedValue = _context?.FormattedValue ?? $"{Value}%";

        if (GetAriaValueText is not null)
            return GetAriaValueText(formattedValue, Value);

        if (!string.IsNullOrEmpty(Format))
            return formattedValue;

        // Base UI default: simple "{value}%" string concatenation.
        return $"{Value}%";
    }

    protected override MeterState GetCurrentState() => new(Value, Min, Max);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield break;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (_context is null) return;

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

        // ARIA meter semantics — matches Base UI's root props.
        builder.AddAttribute(5, "role", "meter");
        builder.AddAttribute(6, "aria-valuenow", Value);
        builder.AddAttribute(7, "aria-valuemin", Min);
        builder.AddAttribute(8, "aria-valuemax", Max);
        builder.AddAttribute(9, "aria-valuetext", ComputeAriaValueText());

        // Set only when a label has registered itself — avoids emitting an empty attribute.
        if (_context.LabelId is { } labelId)
            builder.AddAttribute(10, "aria-labelledby", labelId);

        // Cascade context to Track, Indicator, Value, and Label children.
        builder.OpenComponent<CascadingValue<MeterContext>>(11);
        builder.AddComponentParameter(12, "Value", _context);
        builder.AddComponentParameter(13, "ChildContent", ChildContent);
        builder.CloseComponent();

        // Visually hidden marker required by NVDA to announce the label correctly.
        // Mirrors Base UI's internal <span role="presentation"> element.
        // See: https://github.com/mui/base-ui/issues/4184
        builder.OpenElement(14, "span");
        builder.AddAttribute(15, "role", "presentation");
        builder.AddAttribute(16, "style",
            "clip-path:inset(50%);overflow:hidden;white-space:nowrap;" +
            "border:0;padding:0;width:1px;height:1px;margin:-1px;" +
            "position:fixed;top:0;left:0");
        builder.AddContent(17, "x");
        builder.CloseElement();

        builder.CloseElement();
    }
}

/// <summary>
/// State record shared across all Meter sub-parts for <see cref="BlazeElement{TState}"/>
/// class builders and data-attribute callbacks.
/// </summary>
public readonly record struct MeterState(double Value, double Min, double Max);
