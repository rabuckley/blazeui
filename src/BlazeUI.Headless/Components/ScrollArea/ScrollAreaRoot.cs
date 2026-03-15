using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.ScrollArea;

/// <summary>
/// Groups all parts of the scroll area.
/// Renders a <c>&lt;div&gt;</c> element.
/// </summary>
/// <remarks>
/// The root sets <c>position: relative</c> and the CSS custom properties
/// <c>--scroll-area-corner-height</c> / <c>--scroll-area-corner-width</c> on its
/// inline style so that the absolutely-positioned scrollbar tracks can reference
/// them. Those variables are updated at runtime by the JS module once the corner
/// element has been measured.
/// </remarks>
public class ScrollAreaRoot : BlazeElement<ScrollAreaRootState>, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private BlazeUIConfiguration Config { get; set; } = default!;

    /// <summary>
    /// The threshold in pixels that must be passed before the overflow-edge data
    /// attributes (<c>data-overflow-x-start</c>, <c>data-overflow-x-end</c>,
    /// <c>data-overflow-y-start</c>, <c>data-overflow-y-end</c>) are applied.
    /// Accepts a single value applied to all edges.
    /// </summary>
    [Parameter] public int OverflowEdgeThreshold { get; set; } = 0;

    private const string JavascriptFile = "./_content/BlazeUI.Headless/js/scrollarea/scrollarea.js";

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<ScrollAreaRoot>? _dotNetRef;
    private bool _jsInitialized;
    private readonly ScrollAreaContext _context;

    public ScrollAreaRoot()
    {
        _context = new ScrollAreaContext
        {
            RootId = IdGenerator.Next("scrollarea"),
            ViewportId = IdGenerator.Next("scrollarea-viewport"),
        };
    }

    protected override string DefaultTag => "div";

    // Use the context RootId as the element ID so the JS module can locate
    // the root element via document.getElementById(rootId).
    protected override string ElementId => _context.RootId;

    protected override ScrollAreaRootState GetCurrentState() => new();
    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes() => [];

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
                await _jsModule.InvokeVoidAsync("init", _context.ViewportId, _context.RootId, new
                {
                    overflowEdgeThreshold = OverflowEdgeThreshold,
                });
            }
            catch (JSDisconnectedException) { }
            catch (OperationCanceledException) { }
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddAttribute(1, "id", ElementId);
        if (AdditionalAttributes is not null) builder.AddMultipleAttributes(2, AdditionalAttributes);
        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass)) builder.AddAttribute(3, "class", mergedClass);

        // Base UI sets position: relative and initialises the corner CSS vars at zero.
        // The JS module updates these vars once the corner element has been measured so
        // that scrollbar tracks shorten correctly to avoid overlap with the corner.
        var baseStyle = "position: relative; --scroll-area-corner-height: 0px; --scroll-area-corner-width: 0px;";
        var userStyle = Css.Cn(Style, StyleBuilder?.Invoke(state));
        var mergedStyle = string.IsNullOrEmpty(userStyle) ? baseStyle : $"{baseStyle} {userStyle}";
        builder.AddAttribute(4, "style", mergedStyle);

        builder.AddAttribute(5, "role", "presentation");

        builder.OpenComponent<CascadingValue<ScrollAreaContext>>(6);
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

        try { if (module is not null && _jsInitialized) await module.InvokeVoidAsync("dispose", _context.ViewportId); }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }

        try { if (module is not null) await module.DisposeAsync(); }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }

        _dotNetRef?.Dispose();
    }
}

/// <summary>
/// State for <see cref="ScrollAreaRoot"/>. Scroll and overflow state is managed
/// entirely by the JS module via direct DOM mutations; no Blazor-side state is tracked here.
/// </summary>
public readonly record struct ScrollAreaRootState;
