using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Components.Drawer;

/// <summary>
/// Background layer placed before <see cref="DrawerIndent"/>. Sets
/// <c>data-active</c>/<c>data-inactive</c> based on whether any drawer is open
/// in the provider. Simpler than <see cref="DrawerIndent"/> — no CSS variable syncing.
/// </summary>
public class DrawerIndentBackground : BlazeElement<DrawerIndentBackgroundState>
{
    [CascadingParameter] internal DrawerProviderContext? ProviderContext { get; set; }

    protected override string DefaultTag => "div";

    private bool IsActive => ProviderContext?.Active is true;

    protected override DrawerIndentBackgroundState GetCurrentState() => new(IsActive);

    protected override IEnumerable<KeyValuePair<string, object?>> GetDataAttributes()
    {
        yield return new("data-active", IsActive ? "" : null);
        yield return new("data-inactive", !IsActive ? "" : null);
    }
}

public readonly record struct DrawerIndentBackgroundState(bool Active);
