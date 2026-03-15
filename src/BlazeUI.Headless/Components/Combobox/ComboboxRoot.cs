using BlazeUI.Bridge;
using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Combobox;

/// <summary>
/// State container for a combobox. Combines a text input with a filterable listbox popup.
/// Owns the JS module for keyboard nav, click-outside, positioning, and item selection.
/// </summary>
public class ComboboxRoot : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private BlazeUIConfiguration Config { get; set; } = default!;
    [Inject] private BrowserMutationQueue MutationQueue { get; set; } = default!;

    [Parameter] public RenderFragment? ChildContent { get; set; }

    // -- Single-selection (default) --

    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }
    [Parameter] public string? DefaultValue { get; set; }

    // -- Multiple-selection --

    /// <summary>
    /// When <c>true</c>, multiple items may be selected simultaneously.
    /// Switches <see cref="Values"/> / <see cref="ValuesChanged"/> for value binding.
    /// </summary>
    [Parameter] public bool Multiple { get; set; }

    [Parameter] public IReadOnlyList<string>? Values { get; set; }
    [Parameter] public EventCallback<IReadOnlyList<string>> ValuesChanged { get; set; }
    [Parameter] public IReadOnlyList<string>? DefaultValues { get; set; }

    // -- Input --

    [Parameter] public string? InputValue { get; set; }
    [Parameter] public EventCallback<string?> InputValueChanged { get; set; }

    /// <summary>Placeholder text shown by <c>Combobox.Value</c> when nothing is selected.</summary>
    [Parameter] public string? Placeholder { get; set; }

    // -- Popup open state --

    [Parameter] public bool? Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public bool DefaultOpen { get; set; }

    // -- Behaviour --

    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public bool OpenOnFocus { get; set; } = true;

    private const string JavascriptFile = "./_content/BlazeUI.Headless/js/combobox/combobox.js";

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<ComboboxRoot>? _dotNetRef;
    private readonly ComponentState<bool> _open;
    private readonly ComponentState<string?> _value;
    private List<string> _values = [];
    private List<string> _valueLabels = [];
    private string? _inputValue;
    private readonly ComboboxContext _context;
    private bool _jsInitialized;

    public ComboboxRoot()
    {
        _open = new ComponentState<bool>(false);
        _value = new ComponentState<string?>(null);
        _context = new ComboboxContext
        {
            InputId = IdGenerator.Next("combobox-input"),
            PopupId = IdGenerator.Next("combobox-popup"),
            ListId = IdGenerator.Next("combobox-list"),
            PositionerId = IdGenerator.Next("combobox-positioner"),
        };
        _context.SetOpen = SetOpenAsync;
        _context.Close = () => SetOpenAsync(false);
        _context.SelectItem = SelectItemAsync;
        _context.SetInputValue = SetInputValueAsync;
        _context.RemoveValue = RemoveValueAsync;
        _context.ClearSelection = ClearSelectionAsync;
    }

    protected override void OnInitialized()
    {
        if (DefaultOpen) _open.SetInternal(true);

        // Initialise single-selection uncontrolled default.
        if (DefaultValue is not null) _value.SetInternal(DefaultValue);

        // Initialise multiple-selection uncontrolled default.
        if (DefaultValues is not null) _values = [.. DefaultValues];
    }

    protected override void OnParametersSet()
    {
        if (Open.HasValue) _open.SetControlled(Open.Value);
        else _open.ClearControlled();

        if (Value is not null) _value.SetControlled(Value);
        else _value.ClearControlled();

        // Sync multiple-selection controlled values.
        if (Values is not null) _values = [.. Values];

        // Sync input value from controlled parameter when provided.
        if (InputValue is not null) _inputValue = InputValue;

        SyncContext();
    }

    // Writes all current state into the context so child components see fresh values.
    private void SyncContext()
    {
        _context.Open = _open.Value;
        _context.Multiple = Multiple;
        _context.SelectedValue = _value.Value;
        _context.SelectedValues = _values;
        _context.SelectedLabels = _valueLabels;
        _context.InputValue = _inputValue;
        _context.Placeholder = Placeholder;
        _context.Disabled = Disabled;
        _context.ReadOnly = ReadOnly;
        _context.Required = Required;
        _context.OpenOnFocus = OpenOnFocus;
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
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task SelectItemAsync(string value, string? label)
    {
        if (Multiple)
        {
            // Toggle: if already selected, deselect; otherwise add.
            var idx = _values.IndexOf(value);
            if (idx >= 0)
            {
                _values.RemoveAt(idx);
                if (idx < _valueLabels.Count) _valueLabels.RemoveAt(idx);
            }
            else
            {
                _values.Add(value);
                _valueLabels.Add(label ?? value);
            }

            _context.SelectedValues = _values;
            _context.SelectedLabels = _valueLabels;

            // Clear filter so the full list is visible after selection.
            _context.FilterValue = null;
            _inputValue = "";
            _context.InputValue = "";

            if (ValuesChanged.HasDelegate)
                await ValuesChanged.InvokeAsync(_values);

            if (InputValueChanged.HasDelegate)
                await InputValueChanged.InvokeAsync("");

            // Keep the popup open for multiple selection.
            SyncContext();
            StateHasChanged();
        }
        else
        {
            _value.SetInternal(value);
            _context.SelectedValue = value;
            _context.SelectedLabel = label;

            // Set input text to the label (or value if no label provided).
            // Clear FilterValue so the full list is visible after selection.
            var displayText = label ?? value;
            _inputValue = displayText;
            _context.InputValue = displayText;
            _context.FilterValue = null;

            if (ValueChanged.HasDelegate)
                await ValueChanged.InvokeAsync(value);

            if (InputValueChanged.HasDelegate)
                await InputValueChanged.InvokeAsync(displayText);

            await SetOpenAsync(false);
        }
    }

    private async Task RemoveValueAsync(string value)
    {
        if (!Multiple) return;

        var idx = _values.IndexOf(value);
        if (idx >= 0)
        {
            _values.RemoveAt(idx);
            if (idx < _valueLabels.Count) _valueLabels.RemoveAt(idx);
        }

        _context.SelectedValues = _values;
        _context.SelectedLabels = _valueLabels;

        if (ValuesChanged.HasDelegate)
            await ValuesChanged.InvokeAsync(_values);

        SyncContext();
        StateHasChanged();
    }

    private async Task ClearSelectionAsync()
    {
        _value.SetInternal(null);
        _context.SelectedValue = null;
        _context.SelectedLabel = null;
        _values = [];
        _valueLabels = [];
        _context.SelectedValues = _values;
        _context.SelectedLabels = _valueLabels;
        _context.FilterValue = null;
        _inputValue = "";
        _context.InputValue = "";

        if (ValueChanged.HasDelegate)
            await ValueChanged.InvokeAsync(null);

        if (ValuesChanged.HasDelegate)
            await ValuesChanged.InvokeAsync(_values);

        if (InputValueChanged.HasDelegate)
            await InputValueChanged.InvokeAsync("");

        SyncContext();
        StateHasChanged();
    }

    private async Task SetInputValueAsync(string? inputValue)
    {
        _inputValue = inputValue;
        _context.InputValue = inputValue;
        // User is actively typing — set FilterValue to trigger item filtering.
        _context.FilterValue = inputValue;

        if (InputValueChanged.HasDelegate)
            await InputValueChanged.InvokeAsync(inputValue);

        // Open the popup when the user types, if not already open.
        if (!_open.Value)
            await SetOpenAsync(true);

        StateHasChanged();
    }

    internal async Task SetOpenAsync(bool value)
    {
        if (Disabled && value) return;

        var wasOpen = _open.Value;
        _open.SetInternal(value);
        _context.Open = _open.Value;
        if (!value) _context.HighlightedItemId = null;

        if (!value && wasOpen && _jsModule is not null)
            MutationQueue.Enqueue(new HideComboboxMutation
            {
                ElementId = _context.PopupId,
                PositionerId = _context.PositionerId,
                JsModule = _jsModule,
            });

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
