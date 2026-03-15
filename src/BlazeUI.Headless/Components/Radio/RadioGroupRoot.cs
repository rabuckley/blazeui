using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Radio;

public class RadioGroupRoot : BlazeElement<RadioGroupState>, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private BlazeUIConfiguration Config { get; set; } = default!;

    private const string JavascriptFile = "./_content/BlazeUI.Headless/js/radiogroup/radiogroup.js";

    private IJSObjectReference? _jsModule;
    private bool _jsInitialized;
    [Parameter]
    public string? Value { get; set; }

    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    [Parameter]
    public string? DefaultValue { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool ReadOnly { get; set; }

    [Parameter]
    public bool Required { get; set; }

    /// <summary>
    /// The name attribute forwarded to each radio's hidden input for form submission.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    private readonly ComponentState<string?> _value;

    public RadioGroupRoot()
    {
        _value = new ComponentState<string?>(null);
    }

    protected override void OnInitialized()
    {
        if (DefaultValue is not null)
            _value.SetInternal(DefaultValue);
    }

    protected override void OnParametersSet()
    {
        if (Value is not null)
            _value.SetControlled(Value);
        else
            _value.ClearControlled();
    }

    protected override string DefaultTag => "div";

    protected override RadioGroupState GetCurrentState() => new(Disabled);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-disabled", Disabled ? "" : null);
    }

    protected override IEnumerable<KeyValuePair<string, object>> GetExtraAttributes()
    {
        yield return new("role", "radiogroup");

        // Emit aria-required/aria-disabled/aria-readonly only when true, matching Base UI.
        if (Required)
            yield return new("aria-required", "true");
        if (Disabled)
            yield return new("aria-disabled", "true");
        if (ReadOnly)
            yield return new("aria-readonly", "true");
    }

    internal bool IsChecked(string value) => _value.Value == value;

    private async Task Select(string value)
    {
        if (Disabled || ReadOnly) return;

        // Update state FIRST, then notify. StateHasChanged re-renders the root
        // which re-cascades the context, causing ALL child RadioRoots to update
        // their checked/unchecked state — not just the one that was clicked.
        _value.SetInternal(value);
        StateHasChanged();
        if (ValueChanged.HasDelegate)
            await InvokeAsync(() => ValueChanged.InvokeAsync(value));
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", JavascriptFile.FormatUrl(Config));
            _jsInitialized = true;

            try { await _jsModule.InvokeVoidAsync("init", ResolvedId); }
            catch (JSDisconnectedException) { }
            catch (OperationCanceledException) { }
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var state = GetCurrentState();
        var tag = As ?? DefaultTag;
        var attrs = BuildAttributes(state);

        builder.OpenElement(0, tag);
        builder.AddMultipleAttributes(1, attrs);

        // Cascade context to RadioRoot children
        builder.OpenComponent<CascadingValue<RadioGroupContext>>(2);
        builder.AddComponentParameter(3, "Value",
            new RadioGroupContext(IsChecked, Select, Disabled, ReadOnly, Required, Name));
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

public readonly record struct RadioGroupState(bool Disabled);
