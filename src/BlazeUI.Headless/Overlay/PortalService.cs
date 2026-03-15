using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Overlay;

/// <summary>
/// Scoped pub/sub service that allows components to mount render fragments
/// into a top-level <see cref="PortalHost"/> for overlay rendering.
/// </summary>
internal sealed class PortalService
{
    private readonly List<PortalEntry> _entries = [];

    /// <summary>
    /// Raised when the portal entry list changes. The host subscribes to this
    /// to re-render.
    /// </summary>
    public event Action? OnChanged;

    /// <summary>
    /// Set by <see cref="PortalHost"/> while it is rendering. When true,
    /// <see cref="Update"/> suppresses <see cref="OnChanged"/> to prevent
    /// an infinite render loop caused by nested portals (e.g. submenu portals
    /// inside a parent menu's portal content). Each PortalHost render re-renders
    /// nested Portal components, whose <c>OnParametersSet</c> calls Update with
    /// a new RenderFragment delegate. Without this guard the new reference fails
    /// the <c>ReferenceEquals</c> check, fires OnChanged, and queues another
    /// PortalHost render — ad infinitum.
    /// </summary>
    internal bool IsHostRendering { get; set; }

    /// <summary>
    /// Current portal entries, ordered by z-index.
    /// </summary>
    public IReadOnlyList<PortalEntry> Entries => _entries.OrderBy(e => e.ZIndex).ToList();

    /// <summary>
    /// Adds a render fragment to the portal host.
    /// </summary>
    public string Mount(RenderFragment content, int zIndex = 0)
    {
        var id = Guid.NewGuid().ToString("N");
        _entries.Add(new PortalEntry(id, content, zIndex));
        OnChanged?.Invoke();
        return id;
    }

    /// <summary>
    /// Replaces the content of an existing entry. Only fires <see cref="OnChanged"/>
    /// when the update originates outside <see cref="PortalHost"/>'s render cycle
    /// (i.e. from a genuine parent state change). Updates during PortalHost rendering
    /// are artifacts of Blazor re-rendering nested Portal components and would cause
    /// an infinite loop if they triggered a host re-render.
    /// </summary>
    public void Update(string id, RenderFragment content, int zIndex)
    {
        var index = _entries.FindIndex(e => e.Id == id);
        if (index >= 0)
        {
            _entries[index] = new PortalEntry(id, content, zIndex);

            if (IsHostRendering)
                return;

            OnChanged?.Invoke();
        }
    }

    /// <summary>
    /// Sets the active state of a portal entry. Inactive entries render with
    /// <c>display: none</c> so closed overlay content doesn't leave empty
    /// containers in the DOM tree.
    /// </summary>
    public void SetActive(string id, bool active)
    {
        var entry = _entries.Find(e => e.Id == id);
        if (entry is not null && entry.Active != active)
        {
            entry.Active = active;
            if (!IsHostRendering)
                OnChanged?.Invoke();
        }
    }

    /// <summary>
    /// Removes a previously mounted portal entry.
    /// </summary>
    public void Unmount(string id)
    {
        _entries.RemoveAll(e => e.Id == id);
        OnChanged?.Invoke();
    }
}
