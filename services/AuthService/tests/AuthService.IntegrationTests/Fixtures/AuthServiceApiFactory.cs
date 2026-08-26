using System.Security.Cryptography;
using AuthService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.IntegrationTests.Fixtures;

/// <summary>
/// Boots the real API pipeline (routing, validation, exception handling, JWT authentication) against
/// the Testcontainers-backed database instead of the connection string configured via user-secrets.
/// </summary>
public class AuthServiceApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    /// <summary>
    /// Program.cs reads Jwt:PrivateKeyPem to build the signing key before the host is built, which
    /// happens too early for WebApplicationFactory's ConfigureWebHost overrides to reach - so an
    /// ephemeral test-only key is supplied via environment variables instead, which are read at the
    /// same time as any real user-secrets and take precedence over them.
    /// </summary>
    static AuthServiceApiFactory()
    {
        using var rsa = RSA.Create(2048);
        Environment.SetEnvironmentVariable("Jwt__PrivateKeyPem", rsa.ExportRSAPrivateKeyPem());
        Environment.SetEnvironmentVariable("Jwt__Issuer", "auth-service-test");
        Environment.SetEnvironmentVariable("Jwt__Audience", "ecommerce-clients-test");
    }

    public AuthServiceApiFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AuthDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<AuthDbContext>(options => options.UseNpgsql(_connectionString));
        });
    }
}
