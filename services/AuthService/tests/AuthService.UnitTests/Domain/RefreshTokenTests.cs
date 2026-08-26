using AuthService.Domain.Entities;

namespace AuthService.UnitTests.Domain;

public class RefreshTokenTests
{
    [Fact]
    public void Constructor_CreatesRefreshToken_WhenDataIsValid()
    {
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        var refreshToken = new RefreshToken(userId, "token-hash", expiresAt);

        Assert.Equal(userId, refreshToken.UserId);
        Assert.Equal("token-hash", refreshToken.TokenHash);
        Assert.Equal(expiresAt, refreshToken.ExpiresAt);
        Assert.Null(refreshToken.RevokedAt);
        Assert.True(refreshToken.IsActive);
        Assert.NotEqual(Guid.Empty, refreshToken.Id);
    }

    [Fact]
    public void Constructor_Throws_WhenUserIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new RefreshToken(Guid.Empty, "token-hash", DateTime.UtcNow.AddDays(7)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenTokenHashIsEmptyOrWhitespace(string? tokenHash)
    {
        Assert.Throws<ArgumentException>(() => new RefreshToken(Guid.NewGuid(), tokenHash!, DateTime.UtcNow.AddDays(7)));
    }

    [Fact]
    public void Constructor_Throws_WhenExpiresAtIsInThePast()
    {
        Assert.Throws<ArgumentException>(() => new RefreshToken(Guid.NewGuid(), "token-hash", DateTime.UtcNow.AddDays(-1)));
    }

    [Fact]
    public void Revoke_SetsRevokedAtAndMakesTokenInactive()
    {
        var refreshToken = new RefreshToken(Guid.NewGuid(), "token-hash", DateTime.UtcNow.AddDays(7));

        refreshToken.Revoke();

        Assert.NotNull(refreshToken.RevokedAt);
        Assert.False(refreshToken.IsActive);
    }
}
