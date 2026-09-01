using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ProductService.Infrastructure.Persistence;

namespace ProductService.IntegrationTests.Fixtures;

/// <summary>
/// Boots the real API pipeline (routing, validation, exception handling, authentication) against the
/// Testcontainers-backed database instead of the connection string configured via user-secrets. JWT
/// validation is repointed at a test-only RSA key pair so tests can mint their own tokens without
/// depending on auth-service's real private key.
/// </summary>
public class ProductServiceApiFactory : WebApplicationFactory<Program>
{
    private const string TestIssuer = "auth-service";
    private const string TestAudience = "ecommerce-clients";

    private readonly string _connectionString;
    private readonly RSA _signingKey = RSA.Create();

    public ProductServiceApiFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public string CreateAccessToken(string role)
    {
        var handler = new JwtSecurityTokenHandler();
        var credentials = new SigningCredentials(new RsaSecurityKey(_signingKey), SecurityAlgorithms.RsaSha256);
        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: [new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()), new Claim(ClaimTypes.Role, role)],
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return handler.WriteToken(token);
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
        }
    }
}
