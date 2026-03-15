using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Toolbar;

public class ToolbarRoot : BlazeElement<ToolbarState>, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private BlazeUIConfiguration Config { get; set; } = default!;

    [Parameter] public Orientation Orientation { get; set; } = Orientation.Horizontal;
    [Parameter] public bool Disabled { get; set; }

    private const string JavascriptFile = "./_content/BlazeUI.Headless/js/toolbar/toolbar.js";

    private IJSObjectReference? _jsModule;
    private bool _jsInitialized;
    private readonly ToolbarContext _context = new();

    protected override string DefaultTag => "div";
    protected override ToolbarState GetCurrentState() => new(Orientation, Disabled);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-orientation", Orientation is Orientation.Horizontal ? "horizontal" : "vertical");
        yield return new("data-disabled", Disabled ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("role", "toolbar");
        yield return new("aria-orientation", Orientation is Orientation.Horizontal ? "horizontal" : "vertical");
    }

    protected override void OnParametersSet()
    {
        _context.Orientation = Orientation;
        _context.Disabled = Disabled;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", JavascriptFile.FormatUrl(Config));
            _jsInitialized = true;

            var orientation = Orientation is Orientation.Horizontal ? "horizontal" : "vertical";
            try { await _jsModule.InvokeVoidAsync("init", ResolvedId, orientation); }
            catch (JSDisconnectedException) { }
            catch (OperationCanceledException) { }
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var attrs = BuildAttributes(state);
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddMultipleAttributes(1, attrs);

        builder.OpenComponent<CascadingValue<ToolbarContext>>(2);
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

        try { if (module is not null && _jsInitialized) await module.InvokeVoidAsync("dispose", ResolvedId); }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }

        try { if (module is not null) await module.DisposeAsync(); }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }
    }
}

public readonly record struct ToolbarState(Orientation Orientation, bool Disabled);
