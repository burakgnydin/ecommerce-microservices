using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using AppAuthService = AuthService.Application.Services.AuthService;

namespace AuthService.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AppAuthService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
