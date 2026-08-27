using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OrderService.Application.Interfaces;
using OrderService.Infrastructure.ExternalServices;
using OrderService.Infrastructure.Persistence;

namespace OrderService.IntegrationTests.Fixtures;

/// <summary>
/// Boots the real API pipeline (routing, validation, exception handling, authentication) against the
/// Testcontainers-backed database and a stubbed product-service, instead of the real dependencies
/// configured via user-secrets/appsettings. JWT validation is repointed at a test-only RSA key pair
/// so tests can mint their own tokens without depending on auth-service's real private key.
/// </summary>
public class OrderServiceApiFactory : WebApplicationFactory<Program>
{
    private const string TestIssuer = "auth-service";
    private const string TestAudience = "ecommerce-clients";

    private readonly string _connectionString;
    private readonly RSA _signingKey = RSA.Create();

    public StubProductServiceHandler ProductServiceHandler { get; } = new();

    public OrderServiceApiFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public string CreateAccessToken(Guid userId)
    {
        var handler = new JwtSecurityTokenHandler();
        var credentials = new SigningCredentials(new RsaSecurityKey(_signingKey), SecurityAlgorithms.RsaSha256);
        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: [new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())],
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return handler.WriteToken(token);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<OrderDbContext>));
            if (dbDescriptor is not null)
                services.Remove(dbDescriptor);

            services.AddDbContext<OrderDbContext>(options => options.UseNpgsql(_connectionString));

            services.AddHttpClient<IProductCatalogClient, ProductCatalogClient>(client =>
                {
                    client.BaseAddress = new Uri("http://product-service.test/");
                })
                .ConfigurePrimaryHttpMessageHandler(() => ProductServiceHandler);

            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = TestIssuer,
                    ValidAudience = TestAudience,
                    IssuerSigningKey = new RsaSecurityKey(_signingKey),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _signingKey.Dispose();
            ProductServiceHandler.Dispose();
        }
    }
}
