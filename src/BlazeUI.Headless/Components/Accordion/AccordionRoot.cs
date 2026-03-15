using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Accordion;

/// <summary>
/// State container for an accordion. Manages which items are expanded using
/// single or multiple selection with animated height transitions.
/// </summary>
public class AccordionRoot : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private BlazeUIConfiguration Config { get; set; } = default!;

    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public IReadOnlyList<string>? Value { get; set; }
    [Parameter] public EventCallback<IReadOnlyList<string>> ValueChanged { get; set; }
    [Parameter] public IReadOnlyList<string>? DefaultValue { get; set; }
    [Parameter] public bool OpenMultiple { get; set; }
    [Parameter] public bool Disabled { get; set; }
    /// <summary>
    /// Whether keyboard focus wraps from the last trigger back to the first (and vice versa).
    /// </summary>
    [Parameter] public bool LoopFocus { get; set; } = true;
    [Parameter] public Orientation Orientation { get; set; } = Orientation.Vertical;
    [Parameter] public string? Class { get; set; }
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private const string JavascriptFile = "./_content/BlazeUI.Headless/js/accordion/accordion.js";

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<AccordionRoot>? _dotNetRef;
    private readonly ComponentState<IReadOnlyList<string>> _value;
    private readonly AccordionContext _context;
    private bool _jsInitialized;
    private readonly string _rootId;

    public AccordionRoot()
    {
        _value = new ComponentState<IReadOnlyList<string>>(Array.Empty<string>());
        _rootId = IdGenerator.Next("accordion");
        _context = new AccordionContext();
        _context.Toggle = ToggleItemAsync;
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
        _context.OpenItems = _value.Value;
        _context.Multiple = OpenMultiple;
        _context.Disabled = Disabled;
        _context.Orientation = Orientation;
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

            // Initialize orientation-aware keyboard navigation between triggers.
            // Pass loopFocus so the JS handler can wrap focus correctly.
            var orientation = Orientation is Orientation.Horizontal ? "horizontal" : "vertical";
            try { await _jsModule.InvokeVoidAsync("init", _rootId, orientation, LoopFocus); }
            catch (JSDisconnectedException) { }
            catch (OperationCanceledException) { }
        }
    }

    private async Task ToggleItemAsync(string itemValue)
    {
        if (Disabled) return;

        var oldValues = _value.Value;
        List<string> newValues;

        if (OpenMultiple)
        {
            newValues = new List<string>(oldValues);
            if (newValues.Contains(itemValue))
                newValues.Remove(itemValue);
            else
                newValues.Add(itemValue);
        }
        else
        {
            // Single mode: close if already open, otherwise open this one (closing others).
            newValues = oldValues.Contains(itemValue)
                ? new List<string>()
                : new List<string> { itemValue };
        }

        // Determine which items are being closed — they need exit animations.
        // This includes both the explicitly toggled item and any items that
        // close implicitly (e.g. when opening a different item in single mode).
        var closingValues = oldValues.Where(v => !newValues.Contains(v)).ToList();
        foreach (var closing in closingValues)
        {
            if (_context.PanelIdsByValue.TryGetValue(closing, out var panelId))
                _context.ClosingPanelIds.Add(panelId);
        }

        _value.SetInternal(newValues);
        _context.OpenItems = _value.Value;

        if (ValueChanged.HasDelegate)
            await ValueChanged.InvokeAsync(_value.Value);
        StateHasChanged();

        // Trigger JS animations after the render has applied.
        if (_jsModule is not null)
        {
            // Restart entry animations for newly opened panels. Blazor Server's
            // atomic render diff applies hidden removal + data-open in one batch,
            // which can cause the browser to skip the CSS animation. JS restarts it.
            var openingValues = newValues.Where(v => !oldValues.Contains(v));
            foreach (var opening in openingValues)
            {
                if (_context.PanelIdsByValue.TryGetValue(opening, out var panelId))
                {
                    try { await _jsModule.InvokeVoidAsync("openPanel", panelId, false); }
                    catch (JSDisconnectedException) { }
                    catch (OperationCanceledException) { }
                }
            }

            // Close animations for all closing panels (ClosingPanelIds keeps them visible).
            foreach (var closing in closingValues)
            {
                if (_context.PanelIdsByValue.TryGetValue(closing, out var panelId))
                {
                    try { await _jsModule.InvokeVoidAsync("closePanel", panelId, _dotNetRef); }
                    catch (JSDisconnectedException) { }
                    catch (OperationCanceledException) { }
                }
            }
        }
    }

    [JSInvokable]
    public Task OnCloseAnimationComplete(string panelId)
    {
        _context.ClosingPanelIds.Remove(panelId);
        StateHasChanged();
        return Task.CompletedTask;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", _rootId);
        if (AdditionalAttributes is not null) builder.AddMultipleAttributes(2, AdditionalAttributes);
        if (!string.IsNullOrEmpty(Class)) builder.AddAttribute(3, "class", Class);
        builder.AddAttribute(4, "data-orientation", Orientation is Orientation.Horizontal ? "horizontal" : "vertical");
        if (Disabled) builder.AddAttribute(5, "data-disabled", "");
        builder.AddAttribute(6, "role", "region");

        builder.OpenComponent<CascadingValue<AccordionContext>>(7);
        builder.AddComponentParameter(8, "Value", _context);
        builder.AddComponentParameter(9, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        var module = _jsModule;
        _jsModule = null;

        try { if (module is not null && _jsInitialized) await module.InvokeVoidAsync("dispose", _rootId); }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }

        try { if (module is not null) await module.DisposeAsync(); }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }

        _dotNetRef?.Dispose();
    }
}
