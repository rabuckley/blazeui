using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.NumberField;

public class NumberFieldRoot : BlazeElement<NumberFieldState>, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private BlazeUIConfiguration Config { get; set; } = default!;

    [Parameter] public double? Value { get; set; }
    [Parameter] public double? DefaultValue { get; set; }
    [Parameter] public EventCallback<double?> ValueChanged { get; set; }
    [Parameter] public double? Min { get; set; }
    [Parameter] public double? Max { get; set; }
    [Parameter] public double Step { get; set; } = 1;

    /// <summary>
    /// Step size used for Alt+Arrow key decrements/increments. Defaults to <c>0.1</c>.
    /// </summary>
    [Parameter] public double SmallStep { get; set; } = 0.1;

    /// <summary>
    /// Step size used for Shift+Arrow key increments. Defaults to <c>10 * Step</c> when null.
    /// </summary>
    [Parameter] public double? LargeStep { get; set; }

    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public bool Required { get; set; }

    private const string JavascriptFile = "./_content/BlazeUI.Headless/js/numberfield/numberfield.js";

    // A stable instance key is needed so JS dispose() can clean up only this root's
    // steppers and scrub areas without affecting other NumberField instances on the page.
    private readonly string _instanceKey;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<NumberFieldRoot>? _dotNetRef;
    private readonly ComponentState<double?> _value;
    private readonly NumberFieldContext _context;
    private bool _jsInitialized;

    public NumberFieldRoot()
    {
        _instanceKey = IdGenerator.Next("numberfield");
        _value = new ComponentState<double?>(null);
        _context = new NumberFieldContext
        {
            InputId = IdGenerator.Next("numberfield-input"),
            InstanceKey = _instanceKey,
        };
        _context.SetValue = SetValueAsync;
        _context.StepValue = StepValueAsync;
        _context.SetScrubbing = () =>
        {
            _context.IsScrubbing = true;
            StateHasChanged();
        };
        _context.ClearScrubbing = () =>
        {
            _context.IsScrubbing = false;
            StateHasChanged();
        };
    }

    protected override void OnInitialized()
    {
        if (DefaultValue.HasValue)
            _value.SetInternal(DefaultValue.Value);
    }

    protected override void OnParametersSet()
    {
        if (Value.HasValue)
            _value.SetControlled(Value.Value);
        else
            _value.ClearControlled();

        _context.Value = _value.Value;
        _context.Min = Min;
        _context.Max = Max;
        _context.Step = Step;
        _context.SmallStep = SmallStep;
        _context.LargeStep = LargeStep ?? 10 * Step;
        _context.Disabled = Disabled;
        _context.ReadOnly = ReadOnly;
        _context.Required = Required;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);

            try
            {
                _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                    "import", JavascriptFile.FormatUrl(Config));
            }
            catch (JSDisconnectedException) { return; }
            catch (OperationCanceledException) { return; }

            _context.JsModule = _jsModule;
            _context.DotNetRef = _dotNetRef;
            _jsInitialized = true;

            // Re-render so child components (Increment, Decrement, ScrubArea) see the
            // populated JsModule in their own OnAfterRenderAsync calls.
            StateHasChanged();
        }
    }

    [JSInvokable]
    public Task OnStep(int direction) => StepValueAsync(direction);

    private async Task StepValueAsync(int direction)
    {
        if (Disabled || ReadOnly) return;

        var current = _value.Value ?? 0;
        var newValue = current + direction * Step;
        newValue = Clamp(newValue);

        // Update internal state (effective only in uncontrolled mode).
        _value.SetInternal(newValue);
        _context.Value = newValue;

        // Notify the parent — in controlled mode this is how the value actually changes.
        if (ValueChanged.HasDelegate)
            await InvokeAsync(() => ValueChanged.InvokeAsync(newValue));

        StateHasChanged();
    }

    private async Task SetValueAsync(double? raw)
    {
        if (Disabled || ReadOnly) return;

        var clamped = raw.HasValue ? Clamp(raw.Value) : (double?)null;

        _value.SetInternal(clamped);
        _context.Value = clamped;

        if (ValueChanged.HasDelegate)
            await InvokeAsync(() => ValueChanged.InvokeAsync(clamped));

        StateHasChanged();
    }

    private double Clamp(double value)
    {
        if (Min.HasValue && value < Min.Value) return Min.Value;
        if (Max.HasValue && value > Max.Value) return Max.Value;
        return value;
    }

    protected override string DefaultTag => "div";

    protected override NumberFieldState GetCurrentState() =>
        new(_value.Value, Disabled, ReadOnly, Required, _context.IsScrubbing);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-disabled", Disabled ? "" : null);
        yield return new("data-readonly", ReadOnly ? "" : null);
        yield return new("data-required", Required ? "" : null);
        yield return new("data-scrubbing", _context.IsScrubbing ? "" : null);
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

        // Cascade context to children
        builder.OpenComponent<CascadingValue<NumberFieldContext>>(6);
        builder.AddComponentParameter(7, "Value", _context);
        builder.AddComponentParameter(8, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        var module = _jsModule;
        _jsModule = null;

        try { if (module is not null && _jsInitialized) await module.InvokeVoidAsync("dispose", _instanceKey); }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }

        try { if (module is not null) await module.DisposeAsync(); }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }

        _dotNetRef?.Dispose();
    }
}

public readonly record struct NumberFieldState(
    double? Value,
    bool Disabled,
    bool ReadOnly,
    bool Required,
    bool Scrubbing);
