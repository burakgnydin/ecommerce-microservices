using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.Interfaces;
using ProductService.Application.Strategies;
using AppCategoryService = ProductService.Application.Services.CategoryService;
using AppProductService = ProductService.Application.Services.ProductService;

namespace ProductService.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IProductService, AppProductService>();
        services.AddScoped<ICategoryService, AppCategoryService>();

        services.AddKeyedScoped<IStockValidationStrategy, StandardStockValidationStrategy>(StockValidationStrategyKeys.Standard);
        services.AddKeyedScoped<IStockValidationStrategy, PreOrderStockValidationStrategy>(StockValidationStrategyKeys.PreOrder);

        return services;
    }
}
