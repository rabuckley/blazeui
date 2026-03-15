using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazeUI.Headless.Components.Progress;

/// <summary>
/// Groups all parts of the progress bar and provides the task completion status to screen readers.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
public class ProgressRoot : BlazeElement<ProgressRootState>
{
    /// <summary>
    /// The current value. The component is indeterminate when <see langword="null"/>.
    /// </summary>
    [Parameter]
    public double? Value { get; set; }

    /// <summary>
    /// The minimum value.
    /// </summary>
    [Parameter]
    public double Min { get; set; }

    /// <summary>
    /// The maximum value.
    /// </summary>
    [Parameter]
    public double Max { get; set; } = 100;

    protected override string DefaultTag => "div";

    private readonly ProgressContext _context = new();

    protected override void OnInitialized()
    {
        // Wire the label registration callback after the component is initialized so
        // StateHasChanged is safe to call. This mirrors React's useState(undefined) pattern:
        // ProgressLabel calls SetLabelId during its OnInitialized, which triggers a Root
        // re-render so aria-labelledby is emitted on the following cycle.
        _context.SetLabelId = labelId =>
        {
            _context.LabelId = labelId;
            StateHasChanged();
        };
    }

    protected override void OnParametersSet()
    {
        _context.Value = Value;
        _context.Min = Min;
        _context.Max = Max;
        _context.Status = ComputeStatus();

        // Format the current value as a percentage string for display in ProgressValue.
        // When indeterminate, leave empty — ProgressValue handles that case itself.
        _context.FormattedValue = Value.HasValue
            ? (Value.Value / Max).ToString("P", System.Globalization.CultureInfo.CurrentCulture)
            : "";
    }

    private ProgressStatus ComputeStatus()
    {
        if (!Value.HasValue)
            return ProgressStatus.Indeterminate;

        return Value.Value >= Max ? ProgressStatus.Complete : ProgressStatus.Progressing;
    }

    protected override ProgressRootState GetCurrentState() => new(_context.Status);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-indeterminate", _context.Status is ProgressStatus.Indeterminate ? "" : null);
        yield return new("data-progressing", _context.Status is ProgressStatus.Progressing ? "" : null);
        yield return new("data-complete", _context.Status is ProgressStatus.Complete ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("role", "progressbar");
        yield return new("aria-valuemin", (object)Min);
        yield return new("aria-valuemax", (object)Max);

        if (Value.HasValue)
            yield return new("aria-valuenow", (object)Value.Value);

        // Provide a human-readable value text for assistive technology.
        var valueText = Value.HasValue ? _context.FormattedValue : "indeterminate progress";
        yield return new("aria-valuetext", valueText);

        if (_context.LabelId is not null)
            yield return new("aria-labelledby", _context.LabelId);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;
        var attrs = BuildAttributes(state);

        // CSS custom properties expose min/max/value for use in CSS animations and
        // pseudo-element width tricks without requiring a JS layer.
        var cssVars = $"--progress-min:{Min};--progress-max:{Max};";
        if (_context.Percentage.HasValue)
            cssVars += $"--progress-value:{_context.Percentage.Value};";

        // Merge CSS variables before any consumer-supplied style so they can be overridden.
        if (attrs.TryGetValue("style", out var existingStyle))
            attrs["style"] = Css.Cn(cssVars, existingStyle?.ToString());
        else
            attrs["style"] = cssVars;

        builder.OpenElement(0, tag);
        builder.AddMultipleAttributes(1, attrs);

        // Cascade context to Track, Indicator, Label, and Value children.
        builder.OpenComponent<CascadingValue<ProgressContext>>(2);
        builder.AddComponentParameter(3, "Value", _context);
        builder.AddComponentParameter(4, "ChildContent", ChildContent);
        builder.CloseComponent();

        // Visually hidden span forces NVDA to re-read the progressbar label on value change.
        // See: https://github.com/mui/base-ui/issues/4184
        builder.OpenElement(5, "span");
        builder.AddAttribute(6, "role", "presentation");
        builder.AddAttribute(7, "style",
            "clip-path:inset(50%);overflow:hidden;white-space:nowrap;" +
            "border:0;padding:0;width:1px;height:1px;margin:-1px;" +
            "position:fixed;top:0;left:0");
        builder.AddContent(8, "x");
        builder.CloseElement();

        builder.CloseElement();
    }
}

/// <summary>
/// The current completion state of the progress bar.
/// </summary>
public enum ProgressStatus
{
    /// <summary>The current value is unknown — no <c>value</c> was provided.</summary>
    Indeterminate,
    /// <summary>The task is in progress.</summary>
    Progressing,
    /// <summary>The task has reached its maximum value.</summary>
    Complete,
}

/// <summary>
/// The state record shared by all Progress sub-parts.
/// </summary>
public readonly record struct ProgressRootState(ProgressStatus Status);
