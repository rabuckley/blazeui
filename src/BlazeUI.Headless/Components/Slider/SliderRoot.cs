using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Slider;

/// <summary>
/// Groups all parts of the slider. Renders a <c>&lt;div&gt;</c> element.
/// </summary>
public class SliderRoot : BlazeElement<SliderState>, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private BlazeUIConfiguration Config { get; set; } = default!;

    // Single-value slider parameters. For range sliders use Values/DefaultValues.
    [Parameter] public double? Value { get; set; }
    [Parameter] public double? DefaultValue { get; set; }
    [Parameter] public EventCallback<double> ValueChanged { get; set; }

    // Range slider parameters. When Values is set the slider operates in range mode.
    [Parameter] public double[]? Values { get; set; }
    [Parameter] public double[]? DefaultValues { get; set; }
    [Parameter] public EventCallback<double[]> ValuesChanged { get; set; }

    [Parameter] public double Min { get; set; }
    [Parameter] public double Max { get; set; } = 100;
    [Parameter] public double Step { get; set; } = 1;
    [Parameter] public double? LargeStep { get; set; }
    [Parameter] public Orientation Orientation { get; set; }
    [Parameter] public bool Disabled { get; set; }

    private const string JavascriptFile = "./_content/BlazeUI.Headless/js/slider/slider.js";

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<SliderRoot>? _dotNetRef;
    private bool _jsInitialized;

    // Internal uncontrolled state, mirroring ComponentState<T> but for arrays.
    private double[] _internalValues = [];
    private bool _isControlled;

    private readonly SliderContext _context;

    public SliderRoot()
    {
        _context = new SliderContext
        {
            ControlId = IdGenerator.Next("slider-control"),
            RootId = IdGenerator.Next("slider"),
        };

        // The default label id is a stable derivative of the root id, matching Base UI's
        // getDefaultLabelId() convention. It is used as aria-labelledby before a SliderLabel
        // registers, because the label renders inside the root and its id is only known at
        // render time.
        _context.RootLabelId = _context.RootId + "-label";
        _context.SetValue = SetValueAtIndexAsync;
    }

    protected override void OnInitialized()
    {
        // Determine whether we're in range mode and set initial uncontrolled values.
        if (DefaultValues is { Length: > 0 })
            _internalValues = DefaultValues.ToArray();
        else if (DefaultValue.HasValue)
            _internalValues = [DefaultValue.Value];
        else
            _internalValues = [Min];
    }

    protected override void OnParametersSet()
    {
        // Sync controlled state from parent parameters each render.
        if (Values is not null)
        {
            _isControlled = true;
        }
        else if (Value.HasValue)
        {
            _isControlled = true;
        }
        else
        {
            _isControlled = false;
        }

        var resolvedLargeStep = LargeStep ?? Step * 10;
        var currentValues = GetCurrentValues();

        _context.Values = currentValues;
        _context.Min = Min;
        _context.Max = Max;
        _context.Step = Step;
        _context.LargeStep = resolvedLargeStep;
        _context.Orientation = Orientation;
        _context.Disabled = Disabled;

        // Pre-populate stable thumb input ids based on value count so SliderValue's
        // htmlFor attribute is correct even when it renders before SliderThumb in the
        // component tree. Each id matches the convention used in SliderThumb.BuildRenderTree.
        _context.ThumbInputIds.Clear();
        for (var i = 0; i < currentValues.Length; i++)
            _context.ThumbInputIds.Add(_context.ControlId + "-thumb-" + i);
    }

    // Returns the canonical value array, resolving the controlled/uncontrolled mode.
    private double[] GetCurrentValues()
    {
        if (Values is not null) return Values;
        if (Value.HasValue) return [Value.Value];
        return _internalValues;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", JavascriptFile.FormatUrl(Config));
            _context.JsModule = _jsModule;
            _context.DotNetRef = _dotNetRef;
            _jsInitialized = true;

            var thumbInputIds = _context.ThumbInputIds.ToArray();
            await _jsModule.InvokeVoidAsync("init",
                _context.ControlId,
                thumbInputIds,
                new
                {
                    min = Min,
                    max = Max,
                    step = Step,
                    largeStep = LargeStep ?? Step * 10,
                    orientation = Orientation == Orientation.Vertical ? "vertical" : "horizontal",
                    disabled = Disabled,
                },
                _dotNetRef);
        }
    }

    [JSInvokable]
    public async Task OnValueChange(double value, int index)
    {
        var snapped = Math.Round((value - Min) / Step) * Step + Min;
        snapped = Math.Max(Min, Math.Min(Max, snapped));

        var current = GetCurrentValues();
        var next = current.ToArray();

        if (index >= 0 && index < next.Length)
            next[index] = snapped;
        else
            next[0] = snapped;

        if (!_isControlled)
            _internalValues = next;

        _context.Values = next;

        if (ValuesChanged.HasDelegate)
            await ValuesChanged.InvokeAsync(next);

        if (ValueChanged.HasDelegate && next.Length > 0)
            await ValueChanged.InvokeAsync(next[0]);

        StateHasChanged();
    }

    [JSInvokable]
    public Task OnDragStart()
    {
        _context.Dragging = true;
        StateHasChanged();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnThumbSizeMeasured(double size)
    {
        _context.ThumbSize = size;
        StateHasChanged();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnDragEnd()
    {
        _context.Dragging = false;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task SetValueAtIndexAsync(double value, int index) => OnValueChange(value, index);

    protected override string DefaultTag => "div";

    protected override SliderState GetCurrentState()
    {
        var vals = GetCurrentValues();
        return new(vals, Min, Max, Orientation, Disabled, _context.Dragging);
    }

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-orientation", Orientation == Orientation.Vertical ? "vertical" : "horizontal");
        yield return new("data-disabled", Disabled ? "" : null);
        yield return new("data-dragging", _context.Dragging ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        // The root element is a group container. aria-labelledby points to either
        // the registered SliderLabel id or the default derived label id.
        var labelId = _context.LabelId ?? _context.RootLabelId;
        yield return new("role", "group");
        yield return new("aria-labelledby", labelId);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var attrs = BuildAttributes(state);

        var tag = As ?? DefaultTag;
        builder.OpenElement(0, tag);
        builder.AddMultipleAttributes(1, attrs);

        // Cascade SliderContext so all child components (Control, Track, Thumb, etc.) can access it.
        builder.OpenComponent<CascadingValue<SliderContext>>(2);
        builder.AddComponentParameter(3, "Value", _context);
        builder.AddComponentParameter(4, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        var module = _jsModule;
        _jsModule = null;

        try { if (module is not null && _jsInitialized) await module.InvokeVoidAsync("dispose", _context.ControlId); }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }

        try { if (module is not null) await module.DisposeAsync(); }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }

        _dotNetRef?.Dispose();
    }
}

public readonly record struct SliderState(
    double[] Values,
    double Min,
    double Max,
    Orientation Orientation,
    bool Disabled,
    bool Dragging);
