using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Interfaces;
using OrderService.Infrastructure.ExternalServices;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Persistence.Repositories;

namespace OrderService.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrderDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("OrderDb")));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICartRepository, CartRepository>();

        services.AddHttpClient<IProductCatalogClient, ProductCatalogClient>(client =>
            {
                client.BaseAddress = new Uri(configuration["Services:ProductService:BaseUrl"]
                    ?? throw new InvalidOperationException("Services:ProductService:BaseUrl is not configured."));
            })
            .AddStandardResilienceHandler();

        return services;
    }
}
