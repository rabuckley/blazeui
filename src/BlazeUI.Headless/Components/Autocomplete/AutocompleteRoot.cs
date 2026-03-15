using BlazeUI.Bridge;
using BlazeUI.Headless.Components.Combobox;
using BlazeUI.Headless.Core;
using BlazeUI.Headless.Overlay.Mutations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Autocomplete;

/// <summary>
/// State container for an autocomplete. Free-form text input with a suggestion list.
/// Unlike <see cref="Combobox.ComboboxRoot"/>, the selected value IS the input text —
/// selecting an item sets the input text rather than a separate hidden value.
/// Reuses the combobox JS module and cascades <see cref="ComboboxContext"/> so that
/// all Combobox sub-parts (Input, Item, Popup, Positioner, etc.) work directly beneath it.
/// </summary>
public class AutocompleteRoot : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private BlazeUIConfiguration Config { get; set; } = default!;
    [Inject] private BrowserMutationQueue MutationQueue { get; set; } = default!;

    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The controlled input value. Bind with <c>@bind-Value</c> for two-way binding.
    /// </summary>
    [Parameter] public string? Value { get; set; }

    /// <summary>Raised when the input text changes.</summary>
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>
    /// The initial input value for uncontrolled mode.
    /// To use controlled mode, bind <see cref="Value"/> instead.
    /// </summary>
    [Parameter] public string? DefaultValue { get; set; }

    /// <summary>The controlled open state of the suggestion popup.</summary>
    [Parameter] public bool? Open { get; set; }

    /// <summary>Raised when the popup opens or closes.</summary>
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }

    /// <summary>Whether the popup is open by default (uncontrolled).</summary>
    [Parameter] public bool DefaultOpen { get; set; }

    /// <summary>Whether the component should ignore user interaction.</summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Controls how the autocomplete filters items and performs inline text completion.
    /// <list type="bullet">
    ///   <item><c>List</c> (default) — items are filtered as the user types; the input value does not change based on the highlighted item.</item>
    ///   <item><c>Both</c> — items are filtered and the input temporarily shows the highlighted item's text.</item>
    ///   <item><c>Inline</c> — items are not filtered; the input temporarily shows the highlighted item's text.</item>
    ///   <item><c>None</c> — items are not filtered; the input does not change based on the highlighted item.</item>
    /// </list>
    /// </summary>
    [Parameter] public AutocompleteMode Mode { get; set; } = AutocompleteMode.List;

    /// <summary>
    /// Whether the first matching item is highlighted automatically when the user types.
    /// </summary>
    [Parameter] public bool AutoHighlight { get; set; }

    /// <summary>
    /// Whether the highlighted item should be preserved when the pointer leaves the list.
    /// </summary>
    [Parameter] public bool KeepHighlight { get; set; }

    /// <summary>
    /// Whether moving the pointer over items should highlight them.
    /// When <c>false</c>, CSS <c>:hover</c> can be differentiated from the
    /// <c>data-highlighted</c> keyboard-focus state.
    /// </summary>
    [Parameter] public bool HighlightItemOnHover { get; set; } = true;

    /// <summary>
    /// Whether the suggestion popup opens when clicking the input.
    /// Unlike a Combobox, the autocomplete popup opens automatically when the
    /// user types, so this is <c>false</c> by default.
    /// </summary>
    [Parameter] public bool OpenOnInputClick { get; set; }

    private const string JavascriptFile = "./_content/BlazeUI.Headless/js/combobox/combobox.js";

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<AutocompleteRoot>? _dotNetRef;
    private readonly ComponentState<bool> _open;
    private string? _inputValue;
    private readonly ComboboxContext _context;
    private bool _jsInitialized;

    // Inline completion state — tracks the user's actual typed text separately from
    // the input display value (which may temporarily show a highlighted item's text).
    private string? _typedValue;
    private bool _inlineCompletionActive;
    private bool _pendingSelection;
    private int _selectionStart;
    private int _selectionEnd;

    public AutocompleteRoot()
    {
        _open = new ComponentState<bool>(false);
        _context = new ComboboxContext
        {
            InputId = IdGenerator.Next("autocomplete-input"),
            PopupId = IdGenerator.Next("autocomplete-popup"),
            ListId = IdGenerator.Next("autocomplete-list"),
            PositionerId = IdGenerator.Next("autocomplete-positioner"),
        };
        _context.SetOpen = SetOpenAsync;
        _context.Close = () => SetOpenAsync(false);

        // Selecting an item sets the input text (and value) then closes the popup.
        // For autocomplete, label and value are unified — the display text becomes the value.
        _context.SelectItem = (value, label) => SelectItemAsync(label ?? value);
        _context.SetInputValue = SetInputValueAsync;
    }

    protected override void OnInitialized()
    {
        if (DefaultOpen) _open.SetInternal(true);

        // DefaultValue initialises the input text — unlike ComboboxRoot where
        // DefaultValue seeds the hidden selection value, here the input IS the value.
        if (DefaultValue is not null)
        {
            _inputValue = DefaultValue;
            _typedValue = DefaultValue;
        }
    }

    protected override void OnParametersSet()
    {
        if (Open.HasValue) _open.SetControlled(Open.Value);
        else _open.ClearControlled();

        // Value is the controlled input text.
        if (Value is not null)
        {
            _inputValue = Value;
            _typedValue = Value;
        }

        // OpenOnInputClick maps to OpenOnFocus in the shared ComboboxContext — both
        // control whether focus/click on the input opens the popup. Unlike Combobox
        // (where openOnFocus defaults true), Autocomplete opens via typing rather than
        // focus/click, so OpenOnInputClick defaults to false.
        _context.Open = _open.Value;
        _context.InputValue = _inputValue;
        _context.Disabled = Disabled;
        _context.OpenOnFocus = OpenOnInputClick;

        // Propagate the effective aria-autocomplete mode so ComboboxInput can emit
        // the correct value for the mode.
        _context.AutocompleteMode = Mode;
        _context.InlineComplete = Mode is AutocompleteMode.Both or AutocompleteMode.Inline;
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

            if (_open.Value)
                MutationQueue.Enqueue(new ShowComboboxMutation
                {
                    ElementId = _context.PopupId,
                    PositionerId = _context.PositionerId,
                    InputId = _context.InputId,
                    JsModule = _jsModule,
                    DotNetRef = _dotNetRef,
                    Placement = _context.Placement,
                    Offset = _context.PlacementOffset,
                    InlineComplete = _context.InlineComplete,
                });
        }

        await MutationQueue.FlushAsync();

        // Restore the inline completion selection range after Blazor renders.
        // Blazor's value attribute update may have cleared the browser selection.
        if (_pendingSelection && _jsModule is not null)
        {
            _pendingSelection = false;
            await _jsModule.InvokeVoidAsync("setInputSelection",
                _context.InputId, _selectionStart, _selectionEnd);
        }
    }

    [JSInvokable] public Task OnClickOutside() => SetOpenAsync(false);
    [JSInvokable] public Task OnEscapeKey() => SetOpenAsync(false);

    [JSInvokable]
    public Task OnExitAnimationComplete()
    {
        _context.ExitAnimationComplete = true;
        StateHasChanged();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnHighlightChange(string? itemId)
    {
        _context.HighlightedItemId = itemId;

        // Inline completion: when the user arrows to an item, temporarily show
        // its display text in the input. JS already sets the input value and
        // selection range immediately in the keydown handler; here we sync the
        // Blazor state so the next render diff is a no-op for the value attribute
        // (preventing Blazor from overwriting the JS-set value and clearing the
        // selection).
        if (_context.InlineComplete)
        {
            if (itemId is not null && _context.RegisteredItems.TryGetValue(itemId, out var displayText))
            {
                _inlineCompletionActive = true;
                _inputValue = displayText;
                _context.InputValue = displayText;

                // Schedule a selection range restore after Blazor renders, in case
                // the render does touch the input value attribute.
                _pendingSelection = true;
                var typed = _typedValue ?? "";
                if (displayText.StartsWith(typed, StringComparison.OrdinalIgnoreCase))
                {
                    _selectionStart = typed.Length;
                    _selectionEnd = displayText.Length;
                }
                else
                {
                    _selectionStart = 0;
                    _selectionEnd = displayText.Length;
                }
            }
            else
            {
                // Highlight cleared — restore the typed text.
                _inlineCompletionActive = false;
                _inputValue = _typedValue;
                _context.InputValue = _typedValue;
                _pendingSelection = false;
            }
        }

        StateHasChanged();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when an item is selected from the suggestion list.
    /// Sets the input text to the item's display text and closes the popup.
    /// </summary>
    private async Task SelectItemAsync(string displayText)
    {
        _inputValue = displayText;
        _typedValue = displayText;
        _inlineCompletionActive = false;
        _context.InputValue = displayText;
        // Clear filter so the full list is visible next time the popup opens.
        _context.FilterValue = null;

        if (ValueChanged.HasDelegate)
            await ValueChanged.InvokeAsync(displayText);

        await SetOpenAsync(false);
    }

    /// <summary>
    /// Called when the user types in the input. Updates the input value and opens the popup
    /// so filtered suggestions are visible.
    /// </summary>
    private async Task SetInputValueAsync(string? inputValue)
    {
        _inputValue = inputValue;
        _typedValue = inputValue;
        _inlineCompletionActive = false;
        _context.InputValue = inputValue;

        // Only set FilterValue when the mode calls for filtering. In Inline/None modes
        // items are static — filtering is not applied.
        // In Both mode, always filter by the typed text (not inline-completed text).
        _context.FilterValue = Mode is AutocompleteMode.List or AutocompleteMode.Both
            ? inputValue
            : null;

        if (ValueChanged.HasDelegate)
            await ValueChanged.InvokeAsync(inputValue);

        if (!_open.Value)
            await SetOpenAsync(true);

        StateHasChanged();
    }

    internal async Task SetOpenAsync(bool value)
    {
        if (Disabled && value) return;

        _open.SetInternal(value);
        _context.Open = _open.Value;
        if (!value)
        {
            _context.HighlightedItemId = null;

            // If inline completion was active (user arrowed to an item but didn't
            // press Enter), restore the input to what they actually typed.
            if (_inlineCompletionActive)
            {
                _inlineCompletionActive = false;
                _inputValue = _typedValue;
                _context.InputValue = _typedValue;
                _pendingSelection = false;
            }

            // Directly enqueue hide to bypass Portal re-render propagation.
            if (_jsModule is not null)
                MutationQueue.Enqueue(new HidePopoverMutation
                {
                    ElementId = _context.PopupId,
                    JsModule = _jsModule,
                });
        }

        if (OpenChanged.HasDelegate)
            await OpenChanged.InvokeAsync(_open.Value);
        StateHasChanged();
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<ComboboxContext>>(0);
        builder.AddComponentParameter(1, "Value", _context);
        builder.AddComponentParameter(2, "ChildContent", ChildContent);
        builder.CloseComponent();
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        var module = _jsModule;
        _jsModule = null;

        try { if (module is not null && _jsInitialized) await module.InvokeVoidAsync("dispose", _context.PopupId); }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }

        try { if (module is not null) await module.DisposeAsync(); }
        catch (JSDisconnectedException) { }
        catch (OperationCanceledException) { }

        _dotNetRef?.Dispose();
    }
}
