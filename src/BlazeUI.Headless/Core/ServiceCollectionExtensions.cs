using BlazeUI.Bridge;
using BlazeUI.Headless.Overlay;
using BlazeUI.Headless.Overlay.Dialog;
using BlazeUI.Headless.Overlay.Toast;
using Microsoft.Extensions.DependencyInjection;

namespace BlazeUI.Headless.Core;

/// <summary>
/// Extension methods for registering BlazeUI services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all BlazeUI services required for the headless component library.
    /// </summary>
    public static IServiceCollection AddBlazeUI(
        this IServiceCollection services,
        Action<BlazeUIConfiguration>? configure = null)
    {
        var config = new BlazeUIConfiguration();
        configure?.Invoke(config);
        services.AddSingleton(config);

        services.AddBlazeUIBridge();

        // Overlay services — scoped so each circuit/connection gets its own instance.
        services.AddScoped<PortalService>();
        services.AddScoped<IDialogService, DialogService>();
        services.AddScoped<IToastService, ToastService>();

        return services;
    }
}
