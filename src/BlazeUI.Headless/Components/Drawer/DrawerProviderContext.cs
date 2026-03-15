namespace BlazeUI.Headless.Components.Drawer;

/// <summary>
/// Shared context cascaded by <see cref="DrawerProvider"/> to coordinate nested
/// drawer state. Tracks which drawers are open and provides an aggregate
/// <see cref="Active"/> flag for indent/background effects.
/// </summary>
internal sealed class DrawerProviderContext
{
    private readonly Dictionary<string, bool> _drawers = new();

    /// <summary>
    /// <c>true</c> if any drawer managed by this provider is currently open.
    /// Used by <see cref="DrawerIndent"/> and <see cref="DrawerIndentBackground"/>
    /// to toggle <c>data-active</c>/<c>data-inactive</c> attributes.
    /// </summary>
    public bool Active => _drawers.ContainsValue(true);

    /// <summary>
    /// Called by <see cref="DrawerRoot"/> when its open state changes.
    /// </summary>
    public void SetDrawerOpen(string drawerId, bool open)
    {
        _drawers[drawerId] = open;
    }

    /// <summary>
    /// Called by <see cref="DrawerRoot"/> on dispose to remove its registration.
    /// </summary>
    public void RemoveDrawer(string drawerId)
    {
        _drawers.Remove(drawerId);
    }
}
