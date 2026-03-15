using BlazeUI.Sonner;

namespace BlazeUI.Sonner.Tests;

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

        service.RegisterProvider();
    }

    [Fact]
    public void Show_AddsToastAndFiresOnChange()
    {
        var service = new ToastService();
        var changeCount = 0;
        service.OnChange += () => changeCount++;

        var id = service.Show("Hello");

        Assert.NotEmpty(id);
        Assert.Equal(1, changeCount);
        Assert.Single(service.Toasts);
        Assert.Equal("Hello", service.Toasts[0].Message);
        Assert.Equal(ToastType.Normal, service.Toasts[0].Type);
    }

    [Fact]
    public void Success_SetsTypeToSuccess()
    {
        var service = new ToastService();
        service.Success("Done!");

        Assert.Equal(ToastType.Success, service.Toasts[0].Type);
    }

    [Fact]
    public void Error_SetsTypeToError()
    {
        var service = new ToastService();
        service.Error("Failed!");

        Assert.Equal(ToastType.Error, service.Toasts[0].Type);
    }

    [Fact]
    public void Warning_SetsTypeToWarning()
    {
        var service = new ToastService();
        service.Warning("Careful!");

        Assert.Equal(ToastType.Warning, service.Toasts[0].Type);
    }

    [Fact]
    public void Info_SetsTypeToInfo()
    {
        var service = new ToastService();
        service.Info("FYI");

        Assert.Equal(ToastType.Info, service.Toasts[0].Type);
    }

    [Fact]
    public void Loading_SetsTypeToLoading()
    {
        var service = new ToastService();
        service.Loading("Processing...");

        var toast = service.Toasts[0];
        Assert.Equal(ToastType.Loading, toast.Type);
    }

    [Fact]
    public void Show_WithOptions_AppliesAllSettings()
    {
        var service = new ToastService();
        var onDismissCalled = false;

        service.Show("Test", new ToastOptions
        {
            Description = "Details here",
            Duration = 8000,
            Dismissible = false,
            Class = "custom-class",
            Style = "color: red",
            Position = Position.TopCenter,
            Important = true,
            RichColors = true,
            OnDismiss = () => onDismissCalled = true,
        });

        var toast = service.Toasts[0];
        Assert.Equal("Details here", toast.Description);
        Assert.Equal(8000, toast.Duration);
        Assert.False(toast.Dismissible);
        Assert.Equal("custom-class", toast.Class);
        Assert.Equal("color: red", toast.Style);
        Assert.Equal(Position.TopCenter, toast.Position);
        Assert.True(toast.Important);
        Assert.True(toast.RichColors);

        toast.OnDismiss?.Invoke();
        Assert.True(onDismissCalled);
    }

    [Fact]
    public void Dismiss_ById_MarksToastForDeletion()
    {
        var service = new ToastService();
        var id = service.Show("Test", new ToastOptions { Duration = int.MaxValue });

        service.Dismiss(id);

        Assert.True(service.Toasts[0].MarkedForDeletion);
    }

    [Fact]
    public void Dismiss_All_MarksAllToastsForDeletion()
    {
        var service = new ToastService();
        service.Show("One", new ToastOptions { Duration = int.MaxValue });
        service.Show("Two", new ToastOptions { Duration = int.MaxValue });

        service.Dismiss();

        Assert.All(service.Toasts, t => Assert.True(t.MarkedForDeletion));
    }

    [Fact]
    public void Dismiss_UnknownId_IsNoOp()
    {
        var service = new ToastService();
        var changeCount = 0;
        service.OnChange += () => changeCount++;

        service.Dismiss("nonexistent");

        // Dismiss with unknown ID returns early without firing OnChange.
        Assert.Empty(service.Toasts);
    }

    [Fact]
    public void Remove_DeletesFromList()
    {
        var service = new ToastService();
        var id = service.Show("Test", new ToastOptions { Duration = int.MaxValue });

        service.Remove(id);

        Assert.Empty(service.Toasts);
    }

    [Fact]
    public void Update_ModifiesExistingToast()
    {
        var service = new ToastService();
        var id = service.Show("Original", new ToastOptions { Duration = int.MaxValue });

        service.Update(id, new ToastOptions
        {
            Type = ToastType.Success,
            Description = "Updated description",
            Duration = 5000,
        });

        var toast = service.Toasts[0];
        Assert.Equal(ToastType.Success, toast.Type);
        Assert.Equal("Updated description", toast.Description);
        Assert.Equal(5000, toast.Duration);
    }

    [Fact]
    public void Update_Message_ChangesToastText()
    {
        var service = new ToastService();
        var id = service.Show("Original", new ToastOptions { Duration = int.MaxValue });

        service.Update(id, new ToastOptions { Message = "Updated" });

        Assert.Equal("Updated", service.Toasts[0].Message);
    }

    [Fact]
    public void Update_UnknownId_IsNoOp()
    {
        var service = new ToastService();
        service.Update("nonexistent", new ToastOptions { Description = "nope" });

        Assert.Empty(service.Toasts);
    }

    [Fact]
    public async Task Promise_TransitionsToSuccessOnResolve()
    {
        var service = new ToastService();
        var tcs = new TaskCompletionSource<string>();

        service.Promise(tcs.Task, new PromiseToastOptions<string>
        {
            Loading = "Loading...",
            Success = result => $"Got: {result}",
            Error = ex => ex.Message,
        });

        Assert.Equal(ToastType.Loading, service.Toasts[0].Type);
        Assert.Equal("Loading...", service.Toasts[0].Message);

        tcs.SetResult("data");
        // Allow async continuation to run.
        await Task.Delay(50, TestContext.Current.CancellationToken);

        Assert.Equal(ToastType.Success, service.Toasts[0].Type);
        Assert.Equal("Got: data", service.Toasts[0].Message);
    }

    [Fact]
    public async Task Promise_TransitionsToErrorOnReject()
    {
        var service = new ToastService();
        var tcs = new TaskCompletionSource<int>();

        service.Promise(tcs.Task, new PromiseToastOptions<int>
        {
            Loading = "Saving...",
            Success = _ => "Saved!",
            Error = ex => $"Failed: {ex.Message}",
        });

        tcs.SetException(new InvalidOperationException("db error"));
        await Task.Delay(50, TestContext.Current.CancellationToken);

        Assert.Equal(ToastType.Error, service.Toasts[0].Type);
        Assert.Equal("Failed: db error", service.Toasts[0].Message);
    }

    [Fact]
    public async Task Promise_InvokesFinallyCallback()
    {
        var service = new ToastService();
        var finallyCalled = false;
        var tcs = new TaskCompletionSource<string>();

        service.Promise(tcs.Task, new PromiseToastOptions<string>
        {
            Loading = "...",
            Success = r => r,
            Error = ex => ex.Message,
            Finally = () => finallyCalled = true,
        });

        tcs.SetResult("ok");
        await Task.Delay(50, TestContext.Current.CancellationToken);

        Assert.True(finallyCalled);
    }

    [Fact]
    public void Dispose_CancelsAllPendingTimers()
    {
        var service = new ToastService();
        service.Show("A", new ToastOptions { Duration = 60_000 });
        service.Show("B", new ToastOptions { Duration = 60_000 });

        // Should not throw.
        service.Dispose();
        Assert.Empty(service.Toasts);
    }

    [Fact]
    public void Custom_AddsToastWithCustomContent()
    {
        var service = new ToastService();
        var id = service.Custom(builder =>
        {
            builder.AddContent(0, "Custom content");
        });

        Assert.NotEmpty(id);
        Assert.NotNull(service.Toasts[0].CustomContent);
    }

    [Fact]
    public void Update_PartialUpdate_PreservesUnchangedBoolFields()
    {
        var service = new ToastService();
        var id = service.Show("Test", new ToastOptions
        {
            Duration = int.MaxValue,
            Dismissible = false,
            Important = true,
        });

        service.Update(id, new ToastOptions { Description = "Updated" });

        var toast = service.Toasts[0];
        Assert.Equal("Updated", toast.Description);
        Assert.False(toast.Dismissible);
        Assert.True(toast.Important);
    }

    [Fact]
    public void Update_ExplicitBoolValues_AreApplied()
    {
        var service = new ToastService();
        var id = service.Show("Test", new ToastOptions
        {
            Duration = int.MaxValue,
            Dismissible = true,
            Important = false,
        });

        service.Update(id, new ToastOptions
        {
            Dismissible = false,
            Important = true,
        });

        var toast = service.Toasts[0];
        Assert.False(toast.Dismissible);
        Assert.True(toast.Important);
    }
}
