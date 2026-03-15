using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.InputOTP;

/// <summary>
/// Headless OTP input component. Renders a hidden native <c>&lt;input&gt;</c> that
/// captures keystrokes and selection, then exposes per-slot state via
/// <see cref="InputOTPContext"/> for styled children to consume.
/// </summary>
/// <remarks>
/// Ported from <see href="https://github.com/guilhermerodz/input-otp">input-otp</see>.
/// The hidden input captures all keyboard and paste events. A JS module tracks
/// selection changes and reports them back so each slot knows whether it is active,
/// has a character, or should show a blinking caret.
/// </remarks>
public class InputOTPRoot : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private BlazeUIConfiguration Config { get; set; } = default!;

    [Parameter, EditorRequired] public int MaxLength { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public string? DefaultValue { get; set; }
    [Parameter] public string? Pattern { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public EventCallback<string> OnComplete { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? ContainerClass { get; set; }
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private const string JavascriptFile = "./_content/BlazeUI.Headless/js/input-otp/input-otp.js";

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<InputOTPRoot>? _dotNetRef;
    private readonly ComponentState<string> _value;
    private readonly InputOTPContext _context;
    private readonly string _containerId;
    private readonly string _inputId;
    private bool _jsInitialized;
    private int _selectionStart;
    private int _selectionEnd;

    public InputOTPRoot()
    {
        _value = new ComponentState<string>("");
        _containerId = IdGenerator.Next("input-otp");
        _inputId = IdGenerator.Next("input-otp-input");
        _context = new InputOTPContext();
    }

    protected override void OnInitialized()
    {
        if (DefaultValue is not null)
            _value.SetInternal(DefaultValue);

        _context.ContainerId = _containerId;
        _context.FocusInput = () =>
        {
            _ = FocusInputAsync();
        };
    }

    protected override void OnParametersSet()
    {
        if (Value is not null) _value.SetControlled(Value);
        else _value.ClearControlled();

        RebuildSlots();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", JavascriptFile.FormatUrl(Config));
            _jsInitialized = true;

            try { await _jsModule.InvokeVoidAsync("init", _inputId, _dotNetRef); }
            catch (JSDisconnectedException) { }
            catch (OperationCanceledException) { }
        }
    }

    private void RebuildSlots()
    {
        var val = _value.Value;
        var slots = new InputOTPSlotState[MaxLength];
        var isFocused = _context.IsFocused;

        for (var i = 0; i < MaxLength; i++)
        {
            var ch = i < val.Length ? val[i] : (char?)null;
            var isActive = isFocused && i >= _selectionStart && i < _selectionEnd;
            var hasFakeCaret = isFocused && ch is null
                && _selectionStart == _selectionEnd
                && _selectionStart == i;
            slots[i] = new InputOTPSlotState(ch, hasFakeCaret, isActive);
        }

        _context.Slots = slots;
    }

    /// <summary>
    /// Called from JS when the hidden input's value changes (keyboard or paste).
    /// </summary>
    [JSInvokable]
    public async Task OnInputValueChanged(string newValue)
    {
        // Clamp to MaxLength.
        if (newValue.Length > MaxLength)
            newValue = newValue[..MaxLength];

        // Pattern validation — reject if the new value doesn't match.
        if (!string.IsNullOrEmpty(Pattern))
        {
            try
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(newValue, Pattern))
                    return;
            }
            catch (System.Text.RegularExpressions.RegexParseException)
            {
                // Invalid pattern — allow the value through.
            }
        }

        _value.SetInternal(newValue);
        RebuildSlots();

        if (ValueChanged.HasDelegate)
            await ValueChanged.InvokeAsync(_value.Value);

        if (_value.Value.Length == MaxLength && OnComplete.HasDelegate)
            await OnComplete.InvokeAsync(_value.Value);

        StateHasChanged();
    }

    /// <summary>
    /// Called from JS when the selection/caret position changes.
    /// </summary>
    [JSInvokable]
    public Task OnSelectionChanged(int start, int end)
    {
        _selectionStart = start;
        _selectionEnd = end;
        RebuildSlots();
        StateHasChanged();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called from JS on focus/blur.
    /// </summary>
    [JSInvokable]
    public Task OnFocusChanged(bool focused)
    {
        _context.IsFocused = focused;

        if (focused)
        {
            // Default selection to the end of the value.
            _selectionStart = _value.Value.Length;
            _selectionEnd = _value.Value.Length;
        }

        RebuildSlots();
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task FocusInputAsync()
    {
        if (_jsModule is null) return;
        try { await _jsModule.InvokeVoidAsync("focusInput", _inputId); }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Outer container — matches the input-otp library's container div.
        // pointer-events: none on the container; the input wrapper has pointer-events: all
        // so clicking anywhere on the container focuses the hidden input.
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", _containerId);
        if (!string.IsNullOrEmpty(ContainerClass))
            builder.AddAttribute(3, "class", ContainerClass);
        builder.AddAttribute(4, "data-disabled", Disabled ? "" : null);
        builder.AddAttribute(6, "data-input-otp-container", "true");
        builder.AddAttribute(5, "style",
            "position:relative;cursor:text;user-select:none;-webkit-user-select:none;pointer-events:none;");

        // Cascaded context for slot children — rendered first in the DOM,
        // matching the reference input-otp library's render order.
        builder.OpenComponent<CascadingValue<InputOTPContext>>(10);
        builder.AddComponentParameter(11, "Value", _context);
        builder.AddComponentParameter(12, "ChildContent", ChildContent);
        builder.CloseComponent();

        // Input wrapper div — sits after children in DOM order (matches reference).
        // pointer-events: none on the wrapper; the input itself has pointer-events: all
        // and covers the whole container via position: absolute + inset: 0.
        builder.OpenElement(20, "div");
        builder.AddAttribute(21, "style",
            "position:absolute;inset:0;pointer-events:none;");

        // Hidden native input — captures keyboard/paste. Visible (opacity: 1) but
        // with transparent color/caret so iOS shows the paste menu.
        builder.OpenElement(22, "input");
        builder.AddAttribute(23, "id", _inputId);
        // data-slot is added by the styled template, not here.
        if (AdditionalAttributes is not null) builder.AddMultipleAttributes(24, AdditionalAttributes);
        if (!string.IsNullOrEmpty(Class)) builder.AddAttribute(25, "class", Class);
        builder.AddAttribute(26, "inputmode", "numeric");
        builder.AddAttribute(27, "autocomplete", "one-time-code");
        builder.AddAttribute(28, "maxlength", MaxLength);
        builder.AddAttribute(29, "value", _value.Value);
        builder.AddAttribute(30, "disabled", Disabled ? (object)true : null);
        builder.AddAttribute(31, "spellcheck", "false");
        builder.AddAttribute(32, "style",
            "position:absolute;inset:0;width:100%;height:100%;display:flex;text-align:left;opacity:1;color:transparent;pointer-events:all;background:transparent;caret-color:transparent;border:0 solid transparent;outline:transparent solid 0;box-shadow:none;line-height:1;letter-spacing:-0.5em;font-size:var(--root-height,32px);font-family:monospace;font-variant-numeric:tabular-nums;");
        builder.CloseElement(); // input

        builder.CloseElement(); // wrapper div

        builder.CloseElement(); // container
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        var module = _jsModule;
        _jsModule = null;

        try { if (module is not null && _jsInitialized) await module.InvokeVoidAsync("dispose", _inputId); }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }

        try { if (module is not null) await module.DisposeAsync(); }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }

        _dotNetRef?.Dispose();
    }
}
