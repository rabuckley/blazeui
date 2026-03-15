using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Menubar;

/// <summary>
/// The container for menus in a menubar. Provides inter-menu arrow-key navigation,
/// roving tabindex across triggers, and coordinated open/close state.
/// </summary>
public class MenubarRoot : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private BlazeUIConfiguration Config { get; set; } = default!;

    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Whether the menubar is modal. When <c>true</c>, the page is inert while a menu is open.
    /// </summary>
    [Parameter] public bool Modal { get; set; } = true;

    /// <summary>
    /// Whether the entire menubar is disabled, preventing interaction with all menus.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// The orientation of the menubar. Determines which arrow keys navigate between menus.
    /// </summary>
    [Parameter] public Orientation Orientation { get; set; } = Orientation.Horizontal;

    /// <summary>
    /// Whether keyboard focus loops back to the first trigger when navigating past the last.
    /// </summary>
    [Parameter] public bool LoopFocus { get; set; } = true;

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private const string JavascriptFile = "./_content/BlazeUI.Headless/js/menubar/menubar.js";

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<MenubarRoot>? _dotNetRef;
    private readonly MenubarContext _context;
    private bool _jsInitialized;

    public MenubarRoot()
    {
        _context = new MenubarContext
        {
            RootId = IdGenerator.Next("menubar"),
        };
        _context.OpenMenu = OpenMenuAsync;
        _context.CloseAll = CloseAllAsync;
        _context.SetHasSubmenuOpen = SetHasSubmenuOpen;
    }

    protected override void OnParametersSet()
    {
        _context.Modal = Modal;
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

            try { await _jsModule.InvokeVoidAsync("init", _context.RootId, _dotNetRef); }
            catch (JSDisconnectedException) { }
            catch (OperationCanceledException) { }
        }
    }

    [JSInvokable]
    public async Task OnNavigateMenu(int direction)
    {
        if (_context.ActiveMenu is null) return;

        var next = _context.GetAdjacentMenu(_context.ActiveMenu, direction);
        if (next is null) return;

        // Close current, open next
        _context.ActiveMenu = next;
        StateHasChanged();

        // Focus the new trigger
        if (_jsModule is not null)
        {
            var triggerId = _context.GetTriggerIdForMenu(next);
            if (triggerId is not null)
            {
                try { await _jsModule.InvokeVoidAsync("focusTrigger", triggerId); }
                catch (JSDisconnectedException) { }
                catch (OperationCanceledException) { }
            }
        }
    }

    [JSInvokable]
    public Task OnEscapeKey()
    {
        _context.ActiveMenu = null;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task OpenMenuAsync(string menuValue)
    {
        _context.ActiveMenu = menuValue;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task CloseAllAsync()
    {
        _context.ActiveMenu = null;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private void SetHasSubmenuOpen(bool value)
    {
        if (_context.HasSubmenuOpen == value) return;
        _context.HasSubmenuOpen = value;
        StateHasChanged();
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var orientationValue = Orientation is Orientation.Horizontal ? "horizontal" : "vertical";

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", _context.RootId);
        builder.AddAttribute(2, "role", "menubar");
        builder.AddAttribute(3, "data-orientation", orientationValue);
        builder.AddAttribute(4, "data-has-submenu-open", _context.HasSubmenuOpen ? "true" : null);
        if (AdditionalAttributes is not null)
            builder.AddMultipleAttributes(5, AdditionalAttributes);
        if (!string.IsNullOrEmpty(Class))
            builder.AddAttribute(6, "class", Class);

        builder.OpenComponent<CascadingValue<MenubarContext>>(7);
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

        try { if (module is not null && _jsInitialized) await module.InvokeVoidAsync("dispose", _context.RootId); }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }

        try { if (module is not null) await module.DisposeAsync(); }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }

        _dotNetRef?.Dispose();
    }
}
