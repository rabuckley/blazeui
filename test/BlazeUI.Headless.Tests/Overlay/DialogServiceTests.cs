using BlazeUI.Headless.Overlay.Dialog;
using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Tests.Overlay;

// Minimal component for testing — dialog service just needs a ComponentBase type.
file sealed class StubDialog : ComponentBase;

public class DialogServiceTests
{
    [Fact]
    public void RegisterProvider_ThrowsOnDoubleRegistration()
    {
        var service = new DialogService();
        service.RegisterProvider();

        Assert.Throws<InvalidOperationException>(() => service.RegisterProvider());
    }

    [Fact]
    public void RegisterProvider_AllowsReRegistrationAfterUnregister()
    {
        var service = new DialogService();
        service.RegisterProvider();
        service.UnregisterProvider();

        // Should not throw.
        service.RegisterProvider();
    }

    [Fact]
    public async Task ShowAsync_ReturnsAwaitableTask()
    {
        var service = new DialogService();
        DialogReference? shownReference = null;
        service.OnShow += r => shownReference = r;

        var task = service.ShowAsync<StubDialog>();

        Assert.NotNull(shownReference);
        Assert.False(task.IsCompleted);

        // Simulate dismissal.
        service.Close(shownReference, DialogResult.Ok("done"));

        var result = await task;
        Assert.False(result.Canceled);
        Assert.Equal("done", result.Data);
    }

    [Fact]
    public async Task ShowAsync_DismissCancelsTask()
    {
        var service = new DialogService();
        DialogReference? shownReference = null;
        service.OnShow += r => shownReference = r;

        var task = service.ShowAsync<StubDialog>();

        service.Close(shownReference!, DialogResult.Cancel());

        var result = await task;
        Assert.True(result.Canceled);
    }

    [Fact]
    public async Task Dispose_CancelsAllPendingDialogs()
    {
        var service = new DialogService();
        service.OnShow += _ => { };

        var task1 = service.ShowAsync<StubDialog>();
        var task2 = service.ShowAsync<StubDialog>();

        service.Dispose();

        var result1 = await task1;
        var result2 = await task2;
        Assert.True(result1.Canceled);
        Assert.True(result2.Canceled);
    }
}
