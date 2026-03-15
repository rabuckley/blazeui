using BlazeUI.Headless.Overlay.Toast;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Tests.Overlay;

// Minimal component for testing — toast service just needs a ComponentBase type.
file sealed class StubToast : ComponentBase;

public class ToastServiceTests
{
    [Fact]
    public void RegisterProvider_ThrowsOnDoubleRegistration()
    {
        var service = new ToastService();
        service.RegisterProvider();

        Assert.Throws<InvalidOperationException>(() => service.RegisterProvider());
    }

    [Fact]
    public void RegisterProvider_AllowsReRegistrationAfterUnregister()
    {
        var service = new ToastService();
        service.RegisterProvider();
        service.UnregisterProvider();

        // Should not throw.
        service.RegisterProvider();
    }

    [Fact]
    public void Show_RaisesOnShowEvent()
    {
        var service = new ToastService();
        ToastReference? shown = null;
        service.OnShow += r => shown = r;

        service.Show<StubToast>(new ToastParameters { Timeout = null });

        Assert.NotNull(shown);
    }

    [Fact]
    public void Dismiss_RaisesOnCloseEvent()
    {
        var service = new ToastService();
        ToastReference? shown = null;
        ToastReference? closed = null;
        service.OnShow += r => shown = r;
        service.OnClose += r => closed = r;

        service.Show<StubToast>(new ToastParameters { Timeout = null });
        service.Dismiss(shown!.Id);

        Assert.NotNull(closed);
        Assert.Equal(shown.Id, closed.Id);
    }

    [Fact]
    public void Dismiss_UnknownId_IsNoOp()
    {
        var service = new ToastService();
        var closedCount = 0;
        service.OnClose += _ => closedCount++;

        service.Dismiss("nonexistent-id");

        Assert.Equal(0, closedCount);
    }

    [Fact]
    public void Dispose_CancelsAllPendingTimers()
    {
        var service = new ToastService();
        service.OnShow += _ => { };

        // Show toasts with auto-dismiss so they have CTS instances.
        service.Show<StubToast>(new ToastParameters { Timeout = TimeSpan.FromHours(1) });
        service.Show<StubToast>(new ToastParameters { Timeout = TimeSpan.FromHours(1) });

        // Should not throw — all timers cancelled cleanly.
        service.Dispose();
    }

    [Fact]
    public async Task AutoDismiss_FiresOnCloseAfterTimeout()
    {
        var service = new ToastService();
        ToastReference? closed = null;
        service.OnShow += _ => { };
        service.OnClose += r => closed = r;

        service.Show<StubToast>(new ToastParameters { Timeout = TimeSpan.FromMilliseconds(50) });

        // Wait for the auto-dismiss to fire.
        await Task.Delay(200, TestContext.Current.CancellationToken);

        Assert.NotNull(closed);
    }
}
