using BlazeUI.Bridge;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Bridge.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddBlazeUIBridge_RegistersBrowserMutationQueue()
    {
        var services = new ServiceCollection();

        services.AddBlazeUIBridge();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var queue = scope.ServiceProvider.GetService<BrowserMutationQueue>();

        Assert.NotNull(queue);
    }
}
