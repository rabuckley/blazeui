using BlazeUI.Headless.Core;
using BlazeUI.Headless.Overlay.Mutations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Select;

/// <summary>
/// State container for a select. Owns JS module for keyboard nav, click-outside,
/// positioning, and item selection.
/// </summary>
public class SelectRoot : OverlayRoot
{
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }
    [Parameter] public string? DefaultValue { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public string? Placeholder { get; set; }

    /// <summary>
    /// Form field name. When set, a hidden <c>&lt;input&gt;</c> is rendered
    /// with the current value so the select participates in form submission.
    /// The hidden input is always rendered (for accessibility); the <c>name</c>
    /// attribute is only applied when this parameter is set.
    /// </summary>
    [Parameter] public string? Name { get; set; }

    private DotNetObjectReference<SelectRoot>? _dotNetRef;
    private readonly ComponentState<string?> _value = new(null);
    private readonly SelectContext _context;

    private protected override string ModulePath => "./_content/BlazeUI.Headless/js/select/select.js";
    private protected override string JsInstanceKey => _context.PopupId;

    public SelectRoot()
    {
        _context = new SelectContext
        {
            TriggerId = IdGenerator.Next("select-trigger"),
            PopupId = IdGenerator.Next("select-popup"),
        };
        _context.SetOpen = SetOpenAsync;
        _context.Close = () => SetOpenAsync(false);
        _context.SelectItem = SelectItemAsync;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (DefaultValue is not null) _value.SetInternal(DefaultValue);
    }

    private protected override void SyncContextState() => _context.Open = OpenValue;

    private protected override void OnParametersSetCore()
    {
        if (Value is not null) _value.SetControlled(Value);
        else _value.ClearControlled();

        _context.SelectedValue = _value.Value;
        _context.Placeholder = Placeholder;
        _context.Disabled = Disabled;
        _context.ReadOnly = ReadOnly;
        _context.Required = Required;
    }

    private protected override Task OnJsInitializedAsync()
    {
        _dotNetRef = DotNetObjectReference.Create(this);
        _context.JsModule = _jsModule;
        _context.DotNetRef = _dotNetRef;
        return Task.CompletedTask;
    }

    private protected override void OnDispose() => _dotNetRef?.Dispose();

    /// <summary>
    /// Enqueue JS <c>hide()</c> when closing. Flushed in <see cref="OverlayRoot"/>'s
    /// <c>OnAfterRenderAsync</c>. This bypasses Portal re-render propagation which
    /// is unreliable in Blazor Server mode (cascading value reference equality).
    /// </summary>
    private protected override Task OnBeforeCloseAsync()
    {
        if (_jsModule is not null)
            MutationQueue.Enqueue(new HidePopoverMutation
            {
                ElementId = _context.PopupId,
                JsModule = _jsModule,
            });
        return Task.CompletedTask;
    }

    /// <summary>
    /// Disabled/readonly guard: prevent opening when disabled or read-only.
    /// Clear highlight on close.
    /// </summary>
    internal override async Task SetOpenAsync(bool value)
    {
        if ((Disabled || ReadOnly) && value) return;
        if (!value) _context.HighlightedItemId = null;
        await base.SetOpenAsync(value);
    }

    private async Task SelectItemAsync(string value, string? label)
    {
        _value.SetInternal(value);
        _context.SelectedValue = value;
        _context.SelectedLabel = label;

        if (ValueChanged.HasDelegate)
            await ValueChanged.InvokeAsync(value);

        await SetOpenAsync(false);
    }

    [JSInvokable] public Task OnClickOutside() => SetOpenAsync(false);
    [JSInvokable] public Task OnEscapeKey() => SetOpenAsync(false);
    [JSInvokable] public Task OnExitAnimationComplete() { StateHasChanged(); return Task.CompletedTask; }

    [JSInvokable]
    public Task OnHighlightChange(string? itemId)
    {
        _context.HighlightedItemId = itemId;
        StateHasChanged();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called from JS when an item is selected via Enter/Space/Click.
    /// </summary>
    [JSInvokable]
    public async Task OnItemSelected(string itemId)
    {
        // The item ID is the element ID — SelectItem registers its value/label
        // via the context's SelectItem delegate when clicked. But JS keyboard nav
        // uses this callback, so we need to look up by element ID.
        // For JS-driven selection, we fire a DOM click on the item which triggers
        // the Blazor onclick handler. This keeps value resolution in C#.
        // So this method is intentionally a no-op; the click handler does the work.
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<SelectContext>>(0);
        builder.AddComponentParameter(1, "Value", _context);
        builder.AddComponentParameter(2, "ChildContent", ChildContent);
        builder.CloseComponent();

        // Hidden input for form submission and accessibility parity with Base UI.
        builder.OpenElement(3, "input");
        builder.AddAttribute(4, "id", $"{_context.TriggerId}-hidden-input");
        builder.AddAttribute(5, "tabindex", "-1");
        builder.AddAttribute(6, "aria-hidden", "true");
        builder.AddAttribute(7, "value", _value.Value ?? "");
        if (Name is not null)
            builder.AddAttribute(8, "name", Name);
        builder.AddAttribute(9, "style",
            "clip-path:inset(50%);overflow:hidden;white-space:nowrap;border:0;padding:0;width:1px;height:1px;margin:-1px;position:fixed;top:0;left:0");
        builder.CloseElement();
    }
}
