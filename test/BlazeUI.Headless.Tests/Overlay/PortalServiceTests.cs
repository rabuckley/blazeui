using BlazeUI.Headless.Overlay;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Tests.Overlay;

public class PortalServiceTests
{
    [Fact]
    public void Mount_AddsEntryAndRaisesOnChanged()
    {
        var service = new PortalService();
        var changed = false;
        service.OnChanged += () => changed = true;

        RenderFragment content = builder => builder.AddContent(0, "hello");
        var id = service.Mount(content);

        Assert.True(changed);
        Assert.Single(service.Entries);
        Assert.Equal(id, service.Entries[0].Id);
    }

    [Fact]
    public void Unmount_RemovesEntryAndRaisesOnChanged()
    {
        var service = new PortalService();
        RenderFragment content = builder => builder.AddContent(0, "hello");
        var id = service.Mount(content);

        var changed = false;
        service.OnChanged += () => changed = true;
        service.Unmount(id);

        Assert.True(changed);
        Assert.Empty(service.Entries);
    }

    [Fact]
    public void Entries_AreOrderedByZIndex()
    {
        var service = new PortalService();
        RenderFragment content = builder => { };

        var id1 = service.Mount(content, zIndex: 10);
        var id2 = service.Mount(content, zIndex: 1);
        var id3 = service.Mount(content, zIndex: 5);

        var entries = service.Entries;
        Assert.Equal(id2, entries[0].Id);
        Assert.Equal(id3, entries[1].Id);
        Assert.Equal(id1, entries[2].Id);
    }

    [Fact]
    public void Unmount_WithInvalidId_DoesNotThrow()
    {
        var service = new PortalService();
        service.Unmount("nonexistent");
    }

    /// <summary>
    /// Reproduces the infinite render loop that occurs with nested portals
    /// (e.g. a submenu portal inside a parent menu's portal content).
    /// When PortalHost renders, nested Portal.OnParametersSet calls Update
    /// with a new RenderFragment delegate. Without the IsHostRendering guard,
    /// every Update fires OnChanged → PortalHost re-renders → Update → OnChanged…
    /// </summary>
    [Fact]
    public void Update_DuringHostRender_DoesNotFireOnChanged()
    {
        var service = new PortalService();
        RenderFragment content = builder => builder.AddContent(0, "hello");
        var id = service.Mount(content);

        // Simulate PortalHost entering its render phase.
        service.IsHostRendering = true;

        var changeCount = 0;
        service.OnChanged += () => changeCount++;

        // A nested Portal's OnParametersSet calls Update with a new delegate
        // on every PortalHost render — this must NOT fire OnChanged.
        RenderFragment updated = builder => builder.AddContent(0, "hello");
        service.Update(id, updated, 0);

        Assert.Equal(0, changeCount);

        // Content is still replaced (kept fresh for future renders).
        Assert.Same(updated, service.Entries[0].Content);
    }

    [Fact]
    public void Update_OutsideHostRender_FiresOnChanged()
    {
        var service = new PortalService();
        RenderFragment content = builder => builder.AddContent(0, "hello");
        var id = service.Mount(content);

        var changeCount = 0;
        service.OnChanged += () => changeCount++;

        // Update from a genuine parent state change (IsHostRendering is false).
        RenderFragment updated = builder => builder.AddContent(0, "world");
        service.Update(id, updated, 0);

        Assert.Equal(1, changeCount);
    }
}
