using Microsoft.Extensions.DependencyInjection;
using PaymentService.Application.Interfaces;
using AppPaymentService = PaymentService.Application.Services.PaymentService;

namespace PaymentService.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IPaymentService, AppPaymentService>();

        return services;
    }
}
