using BlazeUI.Headless.Components.Toggle;
using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.ToggleGroup;

public class ToggleGroupRoot : BlazeElement<ToggleGroupState>, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private BlazeUIConfiguration Config { get; set; } = default!;

    private const string JavascriptFile = "./_content/BlazeUI.Headless/js/togglegroup/togglegroup.js";

    private IJSObjectReference? _jsModule;
    private bool _jsInitialized;
    [Parameter]
    public IReadOnlyList<string>? Value { get; set; }

    [Parameter]
    public EventCallback<IReadOnlyList<string>> ValueChanged { get; set; }

    [Parameter]
    public IReadOnlyList<string>? DefaultValue { get; set; }

    [Parameter]
    public bool Multiple { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public Orientation Orientation { get; set; } = Orientation.Horizontal;

    private readonly ComponentState<IReadOnlyList<string>> _value;

    public ToggleGroupRoot()
    {
        _value = new ComponentState<IReadOnlyList<string>>(Array.Empty<string>());
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

    protected override ToggleGroupState GetCurrentState() => new(Disabled, Multiple, Orientation);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-orientation", Orientation is Orientation.Horizontal ? "horizontal" : "vertical");
        yield return new("data-disabled", Disabled ? "" : null);
        // Present when multiple toggle buttons can be pressed simultaneously.
        yield return new("data-multiple", Multiple ? "" : null);
    }

    private bool IsPressed(string value) => _value.Value.Contains(value);

    private async Task Toggle(string value)
    {
        if (Disabled) return;

        List<string> newValues;

        if (Multiple)
        {
            newValues = new List<string>(_value.Value);
            if (newValues.Contains(value))
                newValues.Remove(value);
            else
                newValues.Add(value);
        }
        else
        {
            // Single mode: pressing an already-selected item deselects it;
            // pressing a different item selects it and deselects the previous one.
            newValues = _value.Value.Contains(value)
                ? new List<string>()
                : new List<string> { value };
        }

        // Update internal state, trigger a re-render to propagate the new context to
        // child ToggleRoot items, then notify the consumer.
        _value.SetInternal(newValues);
        await InvokeAsync(StateHasChanged);
        if (ValueChanged.HasDelegate)
            await InvokeAsync(() => ValueChanged.InvokeAsync(newValues));
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
        var tag = As ?? DefaultTag;

        builder.OpenElement(0, tag);
        builder.AddAttribute(1, "id", ResolvedId);

        if (AdditionalAttributes is not null)
            builder.AddMultipleAttributes(2, AdditionalAttributes);

        var mergedClass = Css.Cn(Class, ClassBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedClass))
            builder.AddAttribute(3, "class", mergedClass);

        var mergedStyle = Css.Cn(Style, StyleBuilder?.Invoke(state));
        if (!string.IsNullOrEmpty(mergedStyle))
            builder.AddAttribute(4, "style", mergedStyle);

        foreach (var attr in GetDataAttributes())
            if (attr.Value is not null)
                builder.AddAttribute(5, attr.Key, attr.Value);

        builder.AddAttribute(6, "role", "group");

        // Cascade ToggleGroupContext to child ToggleRoot items.
        builder.OpenComponent<CascadingValue<ToggleGroupContext>>(7);
        builder.AddComponentParameter(8, "Value", new ToggleGroupContext(IsPressed, Toggle, Disabled));
        builder.AddComponentParameter(9, "ChildContent", ChildContent);
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

public readonly record struct ToggleGroupState(bool Disabled, bool Multiple, Orientation Orientation);
