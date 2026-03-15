using BlazeUI.Bridge;
using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.NavigationMenu;

/// <summary>
/// Navigation menu with hover-intent triggered dropdowns, keyboard navigation,
/// and shared viewport for content transitions.
/// </summary>
public class NavigationMenuRoot : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private BlazeUIConfiguration Config { get; set; } = default!;
    [Inject] private BrowserMutationQueue MutationQueue { get; set; } = default!;

    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }
    [Parameter] public string? DefaultValue { get; set; }
    [Parameter] public Orientation Orientation { get; set; } = Orientation.Horizontal;
    [Parameter] public int EnterDelay { get; set; } = 200;
    [Parameter] public int ExitDelay { get; set; } = 300;
    [Parameter] public string? Class { get; set; }
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private const string JavascriptFile = "./_content/BlazeUI.Headless/js/navigationmenu/navigationmenu.js";

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<NavigationMenuRoot>? _dotNetRef;
    private readonly ComponentState<string?> _value;
    private readonly NavigationMenuContext _context;
    private bool _jsInitialized;

    public NavigationMenuRoot()
    {
        _value = new ComponentState<string?>(null);
        _context = new NavigationMenuContext
        {
            RootId = IdGenerator.Next("navmenu"),
            ListId = IdGenerator.Next("navmenu-list"),
            ViewportId = IdGenerator.Next("navmenu-viewport"),
        };
        _context.SetActive = SetActiveAsync;
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
            _context.PreviousValue = previousActive;
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

            try
            {
                await _jsModule.InvokeVoidAsync("init", _context.RootId, new
                {
                    enterDelay = EnterDelay,
                    exitDelay = ExitDelay,
                    orientation = Orientation is Orientation.Horizontal ? "horizontal" : "vertical",
                }, _dotNetRef);
            }
            catch (JSDisconnectedException) { }
            catch (OperationCanceledException) { }
        }

        await MutationQueue.FlushAsync();
    }

    [JSInvokable]
    public Task OnHoverEnter(string itemValue) => SetActiveAsync(itemValue);

    [JSInvokable]
    public Task OnHoverExit() => SetActiveAsync(null);

    [JSInvokable]
    public Task OnExitAnimationComplete()
    {
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task SetActiveAsync(string? itemValue)
    {
        var previous = _value.Value;
        _context.PreviousValue = previous;
        _context.ActivationDirection = _context.ComputeDirection(previous, itemValue);
        _value.SetInternal(itemValue);
        _context.ActiveValue = _value.Value;

        if (_jsModule is not null && _jsInitialized)
        {
            if (itemValue is not null)
                MutationQueue.Enqueue(new ShowViewportMutation
                {
                    ElementId = _context.ViewportId,
                    JsModule = _jsModule,
                    ContentId = _context.GetContentId(itemValue),
                });
            else if (previous is not null)
                MutationQueue.Enqueue(new HideViewportMutation
                {
                    ElementId = _context.ViewportId,
                    JsModule = _jsModule,
                    DotNetRef = _dotNetRef!,
                });
        }

        if (ValueChanged.HasDelegate)
            await ValueChanged.InvokeAsync(_value.Value);
        StateHasChanged();
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "nav");
        builder.AddAttribute(1, "id", _context.RootId);
        builder.AddAttribute(2, "data-orientation", Orientation is Orientation.Horizontal ? "horizontal" : "vertical");
        if (AdditionalAttributes is not null)
            builder.AddMultipleAttributes(3, AdditionalAttributes);
        if (!string.IsNullOrEmpty(Class))
            builder.AddAttribute(4, "class", Class);

        builder.OpenComponent<CascadingValue<NavigationMenuContext>>(5);
        builder.AddComponentParameter(6, "Value", _context);
        builder.AddComponentParameter(7, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        var module = _jsModule;
        _jsModule = null;

        try { if (module is not null && _jsInitialized) await module.InvokeVoidAsync("dispose", _context.RootId); }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }

        try { if (module is not null) await module.DisposeAsync(); }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }

        _dotNetRef?.Dispose();
    }
}
