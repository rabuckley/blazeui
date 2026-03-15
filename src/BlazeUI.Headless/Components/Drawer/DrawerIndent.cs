using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Drawer;

/// <summary>
/// Wrapper for the app's main UI content. Applies <c>data-active</c>/<c>data-inactive</c>
/// based on whether any drawer in the <see cref="DrawerProvider"/> is currently open.
/// Use this to apply visual indent/scale effects on the main content when a drawer opens.
/// </summary>
public class DrawerIndent : BlazeElement<DrawerIndentState>
{
    [CascadingParameter] internal DrawerProviderContext? ProviderContext { get; set; }

    protected override string DefaultTag => "div";

    private bool IsActive => ProviderContext?.Active is true;

    protected override DrawerIndentState GetCurrentState() => new(IsActive);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-active", IsActive ? "" : null);
        yield return new("data-inactive", !IsActive ? "" : null);
    }
}

public readonly record struct DrawerIndentState(bool Active);
