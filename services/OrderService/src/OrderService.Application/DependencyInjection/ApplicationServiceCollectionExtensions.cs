using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Interfaces;
using OrderService.Application.Services;
using AppOrderService = OrderService.Application.Services.OrderService;

namespace OrderService.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IOrderService, AppOrderService>();
        services.AddScoped<ICartService, CartService>();

        return services;
    }
}
