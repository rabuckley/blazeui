using BlazeUI.Bridge;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlazeUI.Sonner;

/// <summary>
/// Extension methods for registering BlazeUI.Sonner services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Sonner toast service. Must be called in both server and client <c>Program.cs</c>.
    /// </summary>
    public static IServiceCollection AddBlazeUISonner(this IServiceCollection services)
    {
        services.AddBlazeUIBridge();
        services.TryAddScoped<ToastService>();
        services.TryAddScoped<ISonnerService>(sp => sp.GetRequiredService<ToastService>());
        return services;
    }
}
