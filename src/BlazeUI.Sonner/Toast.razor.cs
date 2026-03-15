using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazeUI.Sonner;

public partial class Toast : IAsyncDisposable
{
    /// <summary>
    /// Must stay in sync with the CSS exit animation duration (sonner-toast-fade-out / swipe-out).
    /// Matches Sonner's TIME_BEFORE_UNMOUNT.
    /// </summary>
    private const int TimeBeforeUnmount = 200;

    private ElementReference _elementRef;
    private string _elementId = $"toast-{Guid.NewGuid():N}";
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<Toast>? _dotNetRef;
    private readonly CancellationTokenSource _disposeCts = new();
    private bool _mounted;
    private bool _swiping;
    private bool _swipeOut;
    private string? _swipeOutDirection;
    private int _initialHeight;
    private int _offset;

    // Auto-dismiss timer state.
    private CancellationTokenSource? _autoDismissCts;
    private int _remainingMs;
    private long _timerStartedAt;
    private bool _paused;
    private int _lastKnownDuration;
    private bool _removeRequested;

    [Parameter, EditorRequired] public ToastState ToastState { get; set; } = default!;
    [Parameter] public int Index { get; set; }
    [Parameter] public int TotalCount { get; set; }
    [Parameter] public bool Expanded { get; set; }
    [Parameter] public int VisibleToasts { get; set; } = 3;
    [Parameter] public Position Position { get; set; }
    [Parameter] public int Gap { get; set; } = 14;
    [Parameter] public bool CloseButton { get; set; }
    [Parameter] public bool RichColors { get; set; }
    [Parameter] public int FrontToastHeight { get; set; }
    [Parameter] public bool PauseOnHover { get; set; }
    [Parameter] public bool DocumentHidden { get; set; }
    [Parameter] public string SectionId { get; set; } = "";

    /// <summary>
    /// Callback to report the measured height of this toast element back to the Toaster.
    /// </summary>
    [Parameter] public EventCallback<(string ToastId, int Height)> OnHeightReported { get; set; }

    /// <summary>
    /// Callback to request the Toaster to remove this toast from the service (after exit animation).
    /// </summary>
    [Parameter] public EventCallback<string> OnRemoveRequested { get; set; }

    /// <summary>
    /// Pre-computed stacking offset in pixels, passed from the Toaster.
    /// </summary>
    [Parameter] public int Offset { get; set; }

    protected override void OnParametersSet()
    {
        _offset = Offset;

        // If the toast has been marked for deletion, begin the exit animation.
        if (ToastState.MarkedForDeletion && !_swipeOut && !_removeRequested)
        {
            _removeRequested = true;
            CancelAutoDismiss();
            _ = RemoveAfterAnimationAsync();
            return;
        }

        // When a promise toast resolves, its type changes from Loading to Success/Error.
        // StartTimer() skipped the Loading type, so _lastKnownDuration is still 0 — start
        // the auto-dismiss timer now that the toast has a dismissible type.
        if (_lastKnownDuration == 0 && ToastState.Type is not ToastType.Loading && _mounted)
        {
            StartTimer();
        }

        // Detect duration changes (e.g. from Update()) and restart the component timer.
        if (_lastKnownDuration != 0 && ToastState.Duration != _lastKnownDuration)
        {
            _lastKnownDuration = ToastState.Duration;
            CancelAutoDismiss();
            _paused = false;
            StartTimer();
        }

        // Pause/resume auto-dismiss based on document visibility.
        if (DocumentHidden && !_paused)
        {
            PauseTimer();
        }
        else if (!DocumentHidden && _paused)
        {
            ResumeTimer();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            _jsModule = await JS.InvokeAsync<IJSObjectReference>(
                "import", "/_content/BlazeUI.Sonner/js/sonner.js");

            // Measure the rendered height and report back to the Toaster.
            var height = await _jsModule.InvokeAsync<int>("measureHeight", _elementId);
            _initialHeight = height;
            await OnHeightReported.InvokeAsync((ToastState.Id, height));

            // Trigger enter animation.
            _mounted = true;
            StateHasChanged();

            // Start auto-dismiss timer (managed in this component, separate from service-level timer).
            StartTimer();

            // Initialize swipe handling in JS.
            await _jsModule.InvokeVoidAsync("initSwipe", _elementId, _dotNetRef, SectionId);
        }
    }

    // -- Auto-dismiss timer (component-level, with pause/resume) --

    private void StartTimer()
    {
        // Loading and infinite-duration toasts don't auto-dismiss.
        if (ToastState.Type is ToastType.Loading) return;
        if (ToastState.Duration is int.MaxValue) return;

        _lastKnownDuration = ToastState.Duration;
        _remainingMs = ToastState.Duration;
        ResumeTimer();
    }

    private void PauseTimer()
    {
        if (_paused) return;
        _paused = true;
        _autoDismissCts?.Cancel();
        _autoDismissCts?.Dispose();
        _autoDismissCts = null;

        // Calculate how much time has elapsed since the timer started.
        if (_timerStartedAt > 0)
        {
            var elapsed = (int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _timerStartedAt);
            _remainingMs = Math.Max(0, _remainingMs - elapsed);
        }
    }

    private void ResumeTimer()
    {
        if (ToastState.Type is ToastType.Loading) return;
        if (_remainingMs <= 0) return;

        _paused = false;
        _timerStartedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _autoDismissCts = new CancellationTokenSource();
        _ = AutoDismissAsync(_autoDismissCts.Token);
    }

    private void CancelAutoDismiss()
    {
        _autoDismissCts?.Cancel();
        _autoDismissCts?.Dispose();
        _autoDismissCts = null;
    }

    private async Task AutoDismissAsync(CancellationToken ct)
    {
        try
        {
            await Task.Delay(_remainingMs, ct);
            if (_removeRequested) return;
            _removeRequested = true;
            ToastState.OnAutoClose?.Invoke();
            ToastState.MarkedForDeletion = true;
            await InvokeAsync(StateHasChanged);
            await RemoveAfterAnimationAsync();
        }
        catch (TaskCanceledException)
        {
            // Paused or dismissed manually.
        }
    }

    // -- User interactions --

    private void OnPointerEnter()
    {
        if (PauseOnHover) PauseTimer();
    }

    private void OnPointerLeave()
    {
        if (PauseOnHover && !ToastState.MarkedForDeletion) ResumeTimer();
    }

    private void OnCloseButtonClick()
    {
        if (!ToastState.Dismissible) return;
        ToastState.OnDismiss?.Invoke();
        SonnerService.Dismiss(ToastState.Id);
    }

    private void OnCancelClick(ToastAction cancel)
    {
        if (!ToastState.Dismissible) return;
        cancel.OnClick();
        SonnerService.Dismiss(ToastState.Id);
    }

    private void OnActionClick(ToastAction action)
    {
        action.OnClick();
        SonnerService.Dismiss(ToastState.Id);
    }

    // -- Swipe dismiss (JS callback) --

    [JSInvokable]
    public async Task OnSwipeDismiss(string direction)
    {
        if (_removeRequested) return;
        _removeRequested = true;
        _swipeOut = true;
        _swipeOutDirection = direction;
        CancelAutoDismiss();
        ToastState.OnDismiss?.Invoke();
        await InvokeAsync(StateHasChanged);
        await RemoveAfterAnimationAsync();
    }

    [JSInvokable]
    public async Task OnSwipeStateChange(bool swiping)
    {
        _swiping = swiping;
        await InvokeAsync(StateHasChanged);
    }

    // -- Exit animation → remove --

    private async Task RemoveAfterAnimationAsync()
    {
        try
        {
            await Task.Delay(TimeBeforeUnmount, _disposeCts.Token);
            await OnRemoveRequested.InvokeAsync(ToastState.Id);
        }
        catch (TaskCanceledException)
        {
            // Component disposed during the exit animation delay.
        }
    }

    // -- Icons --

    private static RenderFragment CloseIcon => builder =>
    {
        builder.OpenElement(0, "svg");
        builder.AddAttribute(1, "xmlns", "http://www.w3.org/2000/svg");
        builder.AddAttribute(2, "width", "12");
        builder.AddAttribute(3, "height", "12");
        builder.AddAttribute(4, "viewBox", "0 0 24 24");
        builder.AddAttribute(5, "fill", "none");
        builder.AddAttribute(6, "stroke", "currentColor");
        builder.AddAttribute(7, "stroke-width", "1.5");
        builder.AddAttribute(8, "stroke-linecap", "round");
        builder.AddAttribute(9, "stroke-linejoin", "round");
        builder.OpenElement(10, "line");
        builder.AddAttribute(11, "x1", "18");
        builder.AddAttribute(12, "y1", "6");
        builder.AddAttribute(13, "x2", "6");
        builder.AddAttribute(14, "y2", "18");
        builder.CloseElement();
        builder.OpenElement(15, "line");
        builder.AddAttribute(16, "x1", "6");
        builder.AddAttribute(17, "y1", "6");
        builder.AddAttribute(18, "x2", "18");
        builder.AddAttribute(19, "y2", "18");
        builder.CloseElement();
        builder.CloseElement();
    };

    private static RenderFragment LoadingSpinner => builder =>
    {
        var seq = 0;
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "sonner-loading-wrapper");
        builder.AddAttribute(seq++, "data-visible", "true");
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "sonner-spinner");
        for (var i = 0; i < 12; i++)
        {
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "sonner-loading-bar");
            builder.CloseElement();
        }
        builder.CloseElement(); // spinner
        builder.CloseElement(); // wrapper
    };

    private static RenderFragment GetDefaultIcon(ToastType type) => builder =>
    {
        var (path, color) = type switch
        {
            ToastType.Success => ("M20 6L9 17l-5-5", "currentColor"),
            ToastType.Error => ("M18 6L6 18M6 6l12 12", "currentColor"),
            ToastType.Warning => ("M12 9v4m0 4h.01M12 2L2 22h20L12 2z", "currentColor"),
            ToastType.Info => ("M12 16v-4m0-4h.01M12 2a10 10 0 100 20 10 10 0 000-20z", "currentColor"),
            _ => ("", ""),
        };

        if (string.IsNullOrEmpty(path)) return;

        builder.OpenElement(0, "svg");
        builder.AddAttribute(1, "xmlns", "http://www.w3.org/2000/svg");
        builder.AddAttribute(2, "viewBox", "0 0 24 24");
        builder.AddAttribute(3, "width", "16");
        builder.AddAttribute(4, "height", "16");
        builder.AddAttribute(5, "fill", "none");
        builder.AddAttribute(6, "stroke", color);
        builder.AddAttribute(7, "stroke-width", "2");
        builder.AddAttribute(8, "stroke-linecap", "round");
        builder.AddAttribute(9, "stroke-linejoin", "round");
        builder.OpenElement(10, "path");
        builder.AddAttribute(11, "d", path);
        builder.CloseElement();
        builder.CloseElement();
    };

    public async ValueTask DisposeAsync()
    {
        CancelAutoDismiss();
        _disposeCts.Cancel();
        _disposeCts.Dispose();

        if (_jsModule is not null)
        {
            try
            {
                await _jsModule.InvokeVoidAsync("disposeSwipe", _elementId);
                await _jsModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Circuit already gone.
            }
            catch (OperationCanceledException)
            {
                // Component disposed during async.
            }
        }

        _dotNetRef?.Dispose();
    }
}
