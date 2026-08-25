using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Infrastructure.Persistence;

namespace ProductService.IntegrationTests.Fixtures;

/// <summary>
/// Boots the real API pipeline (routing, validation, exception handling) against the
/// Testcontainers-backed database instead of the connection string configured via user-secrets.
/// </summary>
public class ProductServiceApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public ProductServiceApiFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ProductDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<ProductDbContext>(options => options.UseNpgsql(_connectionString));
        });
    }
}
