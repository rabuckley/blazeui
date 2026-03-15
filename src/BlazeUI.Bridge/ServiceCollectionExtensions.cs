using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlazeUI.Bridge;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers BlazeUI.Bridge services (mutation queue, event delegation).
    /// Safe to call multiple times — uses <c>TryAdd</c> semantics.
    /// </summary>
    public static IServiceCollection AddBlazeUIBridge(this IServiceCollection services)
    {
        services.TryAddScoped<BrowserMutationQueue>();
        return services;
    }
}
