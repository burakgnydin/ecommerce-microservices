using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PaymentService.Application.Interfaces;
using PaymentService.Infrastructure.ExternalServices;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.IntegrationTests.Fixtures;

/// <summary>
/// Boots the real API pipeline (routing, validation, exception handling, authentication) against the
/// Testcontainers-backed database and a stubbed order-service, instead of the real dependencies
/// configured via user-secrets/appsettings. JWT validation is repointed at a test-only RSA key pair
/// so tests can mint their own tokens without depending on auth-service's real private key.
/// </summary>
public class PaymentServiceApiFactory : WebApplicationFactory<Program>
{
    private const string TestIssuer = "auth-service";
    private const string TestAudience = "ecommerce-clients";

    private readonly string _connectionString;
    private readonly RSA _signingKey = RSA.Create();

    public StubOrderServiceHandler OrderServiceHandler { get; } = new();
    public StubNotificationServiceHandler NotificationServiceHandler { get; } = new();

    public PaymentServiceApiFactory(string connectionString)
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
            var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PaymentDbContext>));
            if (dbDescriptor is not null)
                services.Remove(dbDescriptor);

            services.AddDbContext<PaymentDbContext>(options => options.UseNpgsql(_connectionString));

            services.Configure<OrderServiceOptions>(options => options.ServiceToken = "test-service-token");

            services.AddHttpClient<IOrderClient, OrderClient>(client =>
                {
                    client.BaseAddress = new Uri("http://order-service.test/");
                })
                .ConfigurePrimaryHttpMessageHandler(() => OrderServiceHandler);

            services.AddHttpClient<INotificationClient, NotificationClient>(client =>
                {
                    client.BaseAddress = new Uri("http://notification-service.test/");
                })
                .ConfigurePrimaryHttpMessageHandler(() => NotificationServiceHandler);

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
            OrderServiceHandler.Dispose();
            NotificationServiceHandler.Dispose();
        }
    }
}
