using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace NotificationService.IntegrationTests.Fixtures;

/// <summary>
/// Boots the real API pipeline (routing, validation, exception handling, authentication) with no
/// database involved, since notification-service is stateless. JWT validation is repointed at a
/// test-only RSA key pair so tests can mint their own tokens without depending on auth-service's
/// real private key.
/// </summary>
public class NotificationServiceApiFactory : WebApplicationFactory<Program>
{
    private const string TestIssuer = "auth-service";
    private const string TestAudience = "ecommerce-clients";

    private readonly RSA _signingKey = RSA.Create();

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
