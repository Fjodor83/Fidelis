using Fidelity.Client.Services;
using Fidelity.Client.Services.Interfaces;
using Fidelity.Client.State;
using Fidelity.Client.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Fidelity.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFidelityServices(this IServiceCollection services)
    {
        // State
        services.AddScoped<AppState>();
        services.AddScoped<AuthState>();
        services.AddScoped<NotificationService>();

        // Helpers
         services.AddScoped<StorageHelper>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<ITransazioneService, TransazioneService>();
        services.AddScoped<ICouponService, CouponService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IResponsabileService, ResponsabileService>();

        return services;
    }
}
