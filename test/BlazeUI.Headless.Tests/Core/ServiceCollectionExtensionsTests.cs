using BlazeUI.Headless.Core;
using BlazeUI.Headless.Overlay;
using BlazeUI.Headless.Overlay.Dialog;
using BlazeUI.Headless.Overlay.Toast;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Headless.Tests.Core;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddBlazeUI_RegistersConfiguration()
    {
        var services = new ServiceCollection();

        services.AddBlazeUI();

        var provider = services.BuildServiceProvider();
        var config = provider.GetRequiredService<BlazeUIConfiguration>();
        Assert.NotNull(config);
    }

    [Fact]
    public void AddBlazeUI_AppliesConfigureCallback()
    {
        var services = new ServiceCollection();

        services.AddBlazeUI(c =>
        {
            c.AnimationsEnabled = false;
            c.JsVersionSuffix = "1.0.0";
        });

        var provider = services.BuildServiceProvider();
        var config = provider.GetRequiredService<BlazeUIConfiguration>();
        Assert.False(config.AnimationsEnabled);
        Assert.Equal("1.0.0", config.JsVersionSuffix);
    }

    [Fact]
    public void AddBlazeUI_RegistersPortalService()
    {
        var services = new ServiceCollection();
        services.AddBlazeUI();
        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var portalService = scope.ServiceProvider.GetRequiredService<PortalService>();
        Assert.NotNull(portalService);
    }

    [Fact]
    public void AddBlazeUI_RegistersDialogService()
    {
        var services = new ServiceCollection();
        services.AddBlazeUI();
        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var dialogService = scope.ServiceProvider.GetRequiredService<IDialogService>();
        Assert.NotNull(dialogService);
    }

    [Fact]
    public void AddBlazeUI_RegistersToastService()
    {
        var services = new ServiceCollection();
        services.AddBlazeUI();
        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var toastService = scope.ServiceProvider.GetRequiredService<IToastService>();
        Assert.NotNull(toastService);
    }

}
