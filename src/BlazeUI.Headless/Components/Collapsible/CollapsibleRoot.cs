using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Collapsible;

/// <summary>
/// State container for a collapsible disclosure widget. Renders a <c>&lt;div&gt;</c>
/// and manages open/close state. Consumers supply <see cref="DefaultOpen"/> for
/// uncontrolled usage or bind <see cref="Open"/>/<see cref="OpenChanged"/> for
/// controlled usage.
/// </summary>
public class CollapsibleRoot : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private BlazeUIConfiguration Config { get; set; } = default!;

    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public bool? Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public bool DefaultOpen { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// The HTML element tag to render. Defaults to <c>"div"</c>. Use <c>"li"</c>
    /// when composing with list-based components like <c>SidebarMenuItem</c>,
    /// mirroring Base UI's <c>render</c> prop pattern.
    /// </summary>
    [Parameter] public string As { get; set; } = "div";

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private const string JavascriptFile = "./_content/BlazeUI.Headless/js/collapsible/collapsible.js";

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<CollapsibleRoot>? _dotNetRef;
    private readonly ComponentState<bool> _open;
    private readonly CollapsibleContext _context;
    private bool _jsInitialized;

    public CollapsibleRoot()
    {
        _open = new ComponentState<bool>(false);
        _context = new CollapsibleContext
        {
            TriggerId = IdGenerator.Next("collapsible-trigger"),
            PanelId = IdGenerator.Next("collapsible-panel"),
        };
        _context.Toggle = ToggleAsync;
    }

    protected override void OnInitialized()
    {
        if (DefaultOpen) _open.SetInternal(true);
    }

    protected override void OnParametersSet()
    {
        if (Open.HasValue) _open.SetControlled(Open.Value);
        else _open.ClearControlled();
        _context.Open = _open.Value;
        _context.Disabled = Disabled;
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

            // If initially open, set up the panel without animation. The
            // `initialRender: true` flag tells JS to set --collapsible-panel-height
            // to 'auto' and cancel any CSS animation, matching Base UI's behavior
            // where DefaultOpen panels appear without a slide-down animation.
            if (_open.Value)
            {
                try { await _jsModule.InvokeVoidAsync("open", _context.PanelId, /* initialRender: */ true); }
                catch (JSDisconnectedException) { }
                catch (OperationCanceledException) { }
            }
        }
    }

    [JSInvokable]
    public Task OnCloseAnimationComplete()
    {
        StateHasChanged();
        return Task.CompletedTask;
    }

    internal async Task ToggleAsync()
    {
        if (Disabled) return;

        var newValue = !_open.Value;
        _open.SetInternal(newValue);
        _context.Open = _open.Value;

        if (_jsModule is not null && _jsInitialized)
        {
            try
            {
                if (newValue)
                    await _jsModule.InvokeVoidAsync("open", _context.PanelId);
                else
                    await _jsModule.InvokeVoidAsync("close", _context.PanelId, _dotNetRef);
            }
            catch (JSDisconnectedException) { }
            catch (OperationCanceledException) { }
        }

        if (OpenChanged.HasDelegate)
            await OpenChanged.InvokeAsync(newValue);
        StateHasChanged();
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var open = _context.Open;
        var disabled = _context.Disabled;

        // The root renders an element that carries open/disabled state attributes,
        // matching Base UI's CollapsibleRoot element contract. The tag is
        // configurable via As to support render-as-list-item composition.
        builder.OpenElement(0, As);
        if (AdditionalAttributes is not null) builder.AddMultipleAttributes(1, AdditionalAttributes);
        if (!string.IsNullOrEmpty(Class)) builder.AddAttribute(2, "class", Class);
        if (open) builder.AddAttribute(3, "data-open", "");
        else builder.AddAttribute(4, "data-closed", "");
        if (disabled) builder.AddAttribute(5, "data-disabled", "");

        builder.OpenComponent<CascadingValue<CollapsibleContext>>(6);
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

        try { if (module is not null && _jsInitialized) await module.InvokeVoidAsync("dispose", _context.PanelId); }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }

        try { if (module is not null) await module.DisposeAsync(); }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }

        _dotNetRef?.Dispose();
    }
}
