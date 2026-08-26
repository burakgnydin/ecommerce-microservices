using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.UnitTests.Security;

public class JwtTokenServiceTests
{
    private static JwtTokenService CreateService(RSA rsa, int expirationMinutes = 15)
    {
        var options = new JwtOptions
        {
            Issuer = "auth-service",
            Audience = "ecommerce-clients",
            PrivateKeyPem = rsa.ExportRSAPrivateKeyPem(),
            AccessTokenExpirationMinutes = expirationMinutes
        };

        return new JwtTokenService(Options.Create(options));
    }

    [Fact]
    public void GenerateAccessToken_ProducesRs256TokenContainingUserClaims()
    {
        using var rsa = RSA.Create(2048);
        var service = CreateService(rsa);
        var user = new User("Jane Doe", "jane@example.com", "hashed-password", Role.Admin);

        var token = service.GenerateAccessToken(user);

        var validationParameters = new TokenValidationParameters
        {
            ValidIssuer = "auth-service",
            ValidAudience = "ecommerce-clients",
            IssuerSigningKey = new RsaSecurityKey(rsa)
        };

        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);

        Assert.Equal(SecurityAlgorithms.RsaSha256, ((JwtSecurityToken)validatedToken).Header.Alg);
        Assert.Equal(user.Id.ToString(), principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value);
        Assert.Equal(user.Email, principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value);
        Assert.Equal(Role.Admin.ToString(), principal.FindFirst(ClaimTypes.Role)?.Value);
    }

    [Fact]
    public void GenerateAccessToken_SetsExpirationAccordingToOptions()
    {
        using var rsa = RSA.Create(2048);
        var service = CreateService(rsa, expirationMinutes: 30);
        var user = new User("Jane Doe", "jane@example.com", "hashed-password");

        var token = service.GenerateAccessToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        var expectedExpiry = DateTime.UtcNow.AddMinutes(30);
        Assert.True(Math.Abs((jwt.ValidTo - expectedExpiry).TotalSeconds) < 5);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsDifferentValues_WhenCalledTwice()
    {
        using var rsa = RSA.Create(2048);
        var service = CreateService(rsa);

        var token1 = service.GenerateRefreshToken();
        var token2 = service.GenerateRefreshToken();

        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void HashRefreshToken_ReturnsSameHash_ForSameInput()
    {
        using var rsa = RSA.Create(2048);
        var service = CreateService(rsa);
        var refreshToken = service.GenerateRefreshToken();

        var hash1 = service.HashRefreshToken(refreshToken);
        var hash2 = service.HashRefreshToken(refreshToken);

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void HashRefreshToken_ReturnsDifferentHash_ForDifferentInput()
    {
        using var rsa = RSA.Create(2048);
        var service = CreateService(rsa);

        var hash1 = service.HashRefreshToken(service.GenerateRefreshToken());
        var hash2 = service.HashRefreshToken(service.GenerateRefreshToken());

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void AccessTokenLifetime_MatchesConfiguredExpirationMinutes()
    {
        using var rsa = RSA.Create(2048);
        var service = CreateService(rsa, expirationMinutes: 30);

        Assert.Equal(TimeSpan.FromMinutes(30), service.AccessTokenLifetime);
    }

    [Fact]
    public void RefreshTokenLifetime_MatchesConfiguredExpirationDays()
    {
        using var rsa = RSA.Create(2048);
        var options = new JwtOptions
        {
            Issuer = "auth-service",
            Audience = "ecommerce-clients",
            PrivateKeyPem = rsa.ExportRSAPrivateKeyPem(),
            RefreshTokenExpirationDays = 14
        };
        var service = new JwtTokenService(Options.Create(options));

        Assert.Equal(TimeSpan.FromDays(14), service.RefreshTokenLifetime);
    }
}
