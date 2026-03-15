using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazeUI.Sonner;

public partial class Toaster : IAsyncDisposable
{
    private const int DefaultDuration = 4000;
    private const int DefaultGap = 14;
    private const int ToastWidth = 356;
    private const string DefaultOffset = "24px";
    private const string MobileOffset = "16px";

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<Toaster>? _dotNetRef;
    private string _sectionId = $"sonner-{Guid.NewGuid():N}";
    // TODO: detect RTL from JS
    private string _dir = "ltr";
    private string _actualTheme = "light";
    private bool _expanded;
    private bool _interacting;
    private bool _documentHidden;
    private int _frontToastHeight;
    private int _lastKnownFrontHeight;

    /// <summary>
    /// Heights tracked per toast ID for stacking offset calculations.
    /// </summary>
    private readonly Dictionary<string, int> _heights = new();

    /// <summary>
    /// Cached toast lists grouped by position, rebuilt only when the service fires OnChange.
    /// </summary>
    private readonly Dictionary<Position, List<ToastState>> _positionCache = new();
    private bool _cacheStale = true;

    // -- Parameters (mirror Sonner's ToasterProps) --

    [Parameter] public Position Position { get; set; } = Position.BottomRight;
    [Parameter] public Theme Theme { get; set; } = Theme.Light;
    [Parameter] public bool RichColors { get; set; }
    [Parameter] public bool Expand { get; set; }
    [Parameter] public int VisibleToasts { get; set; } = 3;
    [Parameter] public int Duration { get; set; } = DefaultDuration;
    [Parameter] public int Gap { get; set; } = DefaultGap;
    [Parameter] public string Offset { get; set; } = DefaultOffset;
    [Parameter] public bool CloseButton { get; set; }
    [Parameter] public bool PauseWhenPageIsHidden { get; set; } = true;
    [Parameter] public string AriaLabel { get; set; } = "Notifications";

    /// <summary>
    /// All distinct positions that have at least one active toast, plus the default position.
    /// </summary>
    private IEnumerable<Position> ActivePositions
    {
        get
        {
            EnsurePositionCache();
            return _positionCache.Keys;
        }
    }

    protected override void OnInitialized()
    {
        _service.RegisterProvider();
        _service.OnChange += OnServiceChange;
        _actualTheme = ResolveTheme();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            _jsModule = await JS.InvokeAsync<IJSObjectReference>(
                "import", "/_content/BlazeUI.Sonner/js/sonner.js");
            await _jsModule.InvokeVoidAsync("init", _sectionId, _dotNetRef);
        }
    }

    private void EnsurePositionCache()
    {
        if (!_cacheStale) return;
        _cacheStale = false;

        // Clear existing lists but keep the dictionary entries to reduce allocation.
        foreach (var list in _positionCache.Values)
        {
            list.Clear();
        }

        // Always include the default position so the Toaster renders its <ol>.
        if (!_positionCache.ContainsKey(Position))
        {
            _positionCache[Position] = new List<ToastState>();
        }

        // Build lists in reverse order so the newest toast is at index 0 (front).
        // Sonner's React implementation prepends new toasts to the array,
        // and the CSS stacking logic depends on index 0 being the front toast.
        for (var i = _service.Toasts.Count - 1; i >= 0; i--)
        {
            var toast = _service.Toasts[i];
            var pos = toast.Position ?? Position;
            if (!_positionCache.TryGetValue(pos, out var list))
            {
                list = new List<ToastState>();
                _positionCache[pos] = list;
            }
            list.Add(toast);
        }

        // Remove positions that are no longer active (except the default).
        var stalePositions = _positionCache
            .Where(kv => kv.Value.Count is 0 && kv.Key != Position)
            .Select(kv => kv.Key)
            .ToList();
        foreach (var pos in stalePositions)
        {
            _positionCache.Remove(pos);
        }
    }

    private List<ToastState> GetToastsForPosition(Position position)
    {
        EnsurePositionCache();
        return _positionCache.TryGetValue(position, out var list) ? list : [];
    }

    private string GetToasterStyle(List<ToastState> toastsForPosition)
    {
        int frontHeight;
        if (toastsForPosition.Count > 0 && _heights.TryGetValue(toastsForPosition[0].Id, out var h))
        {
            frontHeight = h;
            _lastKnownFrontHeight = h;
        }
        else
        {
            // New front toast hasn't been measured yet — use the last known
            // front height so non-front toasts keep their correct stacking
            // height until the new measurement arrives.
            frontHeight = _lastKnownFrontHeight;
        }
        _frontToastHeight = frontHeight;

        return $"--front-toast-height: {frontHeight}px; --width: {ToastWidth}px; --gap: {Gap}px; "
             + $"--offset-top: {Offset}; --offset-right: {Offset}; --offset-bottom: {Offset}; --offset-left: {Offset}; "
             + $"--mobile-offset-top: {MobileOffset}; --mobile-offset-right: {MobileOffset}; --mobile-offset-bottom: {MobileOffset}; --mobile-offset-left: {MobileOffset};";
    }

    /// <summary>
    /// Computes the pixel offset for a toast at the given index within its position group.
    /// </summary>
    private int ComputeOffset(List<ToastState> toastsInGroup, int index)
    {
        var offset = 0;
        for (var i = 0; i < index; i++)
        {
            if (_heights.TryGetValue(toastsInGroup[i].Id, out var h))
            {
                offset += h;
            }
            offset += Gap;
        }
        return offset;
    }

    private string ResolveTheme()
    {
        return Theme switch
        {
            Theme.Light => "light",
            Theme.Dark => "dark",
            // System defaults to light on the server; JS will update on the client.
            Theme.System => "light",
            _ => "light",
        };
    }

    // -- Event handlers --

    private void OnMouseEnter() => _expanded = true;
    private void OnMouseMove() => _expanded = true;
    private void OnMouseLeave()
    {
        if (!_interacting) _expanded = false;
    }
    private void OnPointerDown() => _interacting = true;
    private void OnPointerUp() => _interacting = false;

    private void OnServiceChange()
    {
        _cacheStale = true;
        _ = InvokeAsync(StateHasChanged);
    }

    private void OnToastHeightReported((string ToastId, int Height) report)
    {
        _heights[report.ToastId] = report.Height;
        StateHasChanged();
    }

    private void OnToastRemoveRequested(string toastId)
    {
        _heights.Remove(toastId);
        _service.Remove(toastId);
    }

    // -- JS callbacks --

    [JSInvokable]
    public void OnVisibilityChange(bool hidden)
    {
        if (!PauseWhenPageIsHidden) return;
        _documentHidden = hidden;
        _ = InvokeAsync(StateHasChanged);
    }

    [JSInvokable]
    public void OnThemeChange(string theme)
    {
        _actualTheme = theme;
        _ = InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        _service.OnChange -= OnServiceChange;
        _service.UnregisterProvider();

        if (_jsModule is not null)
        {
            try
            {
                await _jsModule.InvokeVoidAsync("dispose", _sectionId);
                await _jsModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Circuit already gone — safe to ignore.
            }
            catch (OperationCanceledException)
            {
                // Component disposed during an async operation.
            }
        }

        _dotNetRef?.Dispose();
    }
}
