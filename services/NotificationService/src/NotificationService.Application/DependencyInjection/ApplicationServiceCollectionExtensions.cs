using Microsoft.Extensions.DependencyInjection;
using AppNotificationService = NotificationService.Application.Services.NotificationService;

namespace NotificationService.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<AppNotificationService>();

        return services;
    }
}
