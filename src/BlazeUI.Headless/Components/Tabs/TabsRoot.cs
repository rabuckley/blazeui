using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Tabs;

/// <summary>
/// State container for a tabbed interface. Renders a <c>&lt;div&gt;</c> element, manages
/// active tab selection with roving tabindex keyboard navigation and optional indicator
/// positioning.
/// </summary>
public class TabsRoot : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private BlazeUIConfiguration Config { get; set; } = default!;

    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }
    [Parameter] public string? DefaultValue { get; set; }
    [Parameter] public Orientation Orientation { get; set; } = Orientation.Horizontal;
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private const string JavascriptFile = "./_content/BlazeUI.Headless/js/tabs/tabs.js";

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<TabsRoot>? _dotNetRef;
    private readonly ComponentState<string?> _value;
    private readonly TabsContext _context;
    private bool _jsInitialized;
    private bool _needsIndicatorUpdate;
    private readonly string _rootId;

    public TabsRoot()
    {
        _value = new ComponentState<string?>(null);
        _rootId = IdGenerator.Next("tabs");
        _context = new TabsContext
        {
            TabListId = IdGenerator.Next("tabs-list"),
        };
        _context.Activate = ActivateAsync;
    }

    protected override void OnInitialized()
    {
        if (DefaultValue is not null)
            _value.SetInternal(DefaultValue);
    }

    protected override void OnParametersSet()
    {
        if (Value is not null) _value.SetControlled(Value);
        else _value.ClearControlled();

        var previousActive = _context.ActiveValue;
        _context.ActiveValue = _value.Value;
        _context.Orientation = Orientation;

        if (previousActive != _context.ActiveValue)
        {
            _context.PreviousValue = previousActive;
            _context.ActivationDirection = ComputeActivationDirection();
            _needsIndicatorUpdate = true;
        }
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

            var orientation = Orientation is Orientation.Horizontal ? "horizontal" : "vertical";
            try { await _jsModule.InvokeVoidAsync("init", _context.TabListId, orientation); }
            catch (JSDisconnectedException) { }
            catch (OperationCanceledException) { }

            _needsIndicatorUpdate = true;
        }

        if (_needsIndicatorUpdate && _jsInitialized && _jsModule is not null && _context.ActiveValue is not null)
        {
            _needsIndicatorUpdate = false;
            try { await _jsModule.InvokeVoidAsync("measureIndicator", _context.TabListId, _context.ActiveValue); }
            catch (JSDisconnectedException) { }
            catch (OperationCanceledException) { }
        }
    }

    private async Task ActivateAsync(string tabValue)
    {
        var previousActive = _value.Value;
        _context.PreviousValue = previousActive;
        _value.SetInternal(tabValue);
        _context.ActiveValue = _value.Value;
        _context.ActivationDirection = ComputeActivationDirection();
        _needsIndicatorUpdate = true;

        if (ValueChanged.HasDelegate)
            await ValueChanged.InvokeAsync(_value.Value);
        StateHasChanged();
    }

    /// <summary>
    /// Computes the direction of the most recent tab activation relative to the previous
    /// selection. Uses tab registration order as a proxy for visual position.
    /// </summary>
    private string ComputeActivationDirection()
    {
        if (_context.PreviousValue is null || _context.ActiveValue is null)
            return "none";

        var prevIndex = _context.GetIndex(_context.PreviousValue);
        var activeIndex = _context.GetIndex(_context.ActiveValue);

        if (prevIndex == -1 || activeIndex == -1) return "none";

        var isHorizontal = Orientation is Orientation.Horizontal;
        if (activeIndex > prevIndex)
            return isHorizontal ? "right" : "down";
        if (activeIndex < prevIndex)
            return isHorizontal ? "left" : "up";
        return "none";
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var orientationValue = Orientation is Orientation.Horizontal ? "horizontal" : "vertical";

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", _rootId);
        if (AdditionalAttributes is not null) builder.AddMultipleAttributes(2, AdditionalAttributes);
        if (!string.IsNullOrEmpty(Class)) builder.AddAttribute(3, "class", Class);
        if (!string.IsNullOrEmpty(Style)) builder.AddAttribute(4, "style", Style);
        builder.AddAttribute(5, "data-orientation", orientationValue);
        // Base UI emits a boolean data-horizontal / data-vertical attribute in addition to
        // data-orientation. Styled templates use group-data-horizontal/tabs selectors to target it.
        builder.AddAttribute(6, $"data-{orientationValue}", "");
        builder.AddAttribute(7, "data-activation-direction", _context.ActivationDirection);

        builder.OpenComponent<CascadingValue<TabsContext>>(8);
        builder.AddComponentParameter(9, "Value", _context);
        builder.AddComponentParameter(10, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        var module = _jsModule;
        _jsModule = null;

        try { if (module is not null && _jsInitialized) await module.InvokeVoidAsync("dispose", _context.TabListId); }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }

        try { if (module is not null) await module.DisposeAsync(); }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }

        _dotNetRef?.Dispose();
    }
}
