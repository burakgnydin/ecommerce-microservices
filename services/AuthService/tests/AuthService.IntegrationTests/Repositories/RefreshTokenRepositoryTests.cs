using AuthService.Domain.Entities;
using AuthService.IntegrationTests.Fixtures;
using AuthService.Infrastructure.Persistence.Repositories;

namespace AuthService.IntegrationTests.Repositories;

[Collection("Integration")]
public class RefreshTokenRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public RefreshTokenRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<User> CreateUserAsync()
    {
        await using var context = _fixture.CreateDbContext();
        var user = new User("Jane Doe", $"jane-{Guid.NewGuid()}@example.com", "hashed-password");
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task CreateAsync_PersistsToken_RetrievableByTokenHash()
    {
        var user = await CreateUserAsync();
        await using var context = _fixture.CreateDbContext();
        var repository = new RefreshTokenRepository(context);
        var token = new RefreshToken(user.Id, "hash-1", DateTime.UtcNow.AddDays(7));

        await repository.CreateAsync(token);

        await using var verifyContext = _fixture.CreateDbContext();
        var retrieved = await new RefreshTokenRepository(verifyContext).GetByTokenHashAsync("hash-1");

        Assert.NotNull(retrieved);
        Assert.Equal(user.Id, retrieved!.UserId);
        Assert.True(retrieved.IsActive);
    }

    [Fact]
    public async Task RevokeAsync_MarksTokenInactive()
    {
        var user = await CreateUserAsync();
        await using var setupContext = _fixture.CreateDbContext();
        var token = new RefreshToken(user.Id, "hash-2", DateTime.UtcNow.AddDays(7));
        await new RefreshTokenRepository(setupContext).CreateAsync(token);

        await using var revokeContext = _fixture.CreateDbContext();
        await new RefreshTokenRepository(revokeContext).RevokeAsync(token.Id);

        await using var verifyContext = _fixture.CreateDbContext();
        var retrieved = await new RefreshTokenRepository(verifyContext).GetByTokenHashAsync("hash-2");

        Assert.NotNull(retrieved);
        Assert.False(retrieved!.IsActive);
    }

    [Fact]
    public async Task RevokeActiveByUserIdAsync_RevokesAllActiveTokens_ForThatUserOnly()
    {
        var user = await CreateUserAsync();
        var otherUser = await CreateUserAsync();

        await using var setupContext = _fixture.CreateDbContext();
        var setupRepository = new RefreshTokenRepository(setupContext);
        var tokenA = new RefreshToken(user.Id, "hash-3a", DateTime.UtcNow.AddDays(7));
        var tokenB = new RefreshToken(user.Id, "hash-3b", DateTime.UtcNow.AddDays(7));
        var otherToken = new RefreshToken(otherUser.Id, "hash-3c", DateTime.UtcNow.AddDays(7));
        await setupRepository.CreateAsync(tokenA);
        await setupRepository.CreateAsync(tokenB);
        await setupRepository.CreateAsync(otherToken);

        await using var revokeContext = _fixture.CreateDbContext();
        await new RefreshTokenRepository(revokeContext).RevokeActiveByUserIdAsync(user.Id);

        await using var verifyContext = _fixture.CreateDbContext();
        var verifyRepository = new RefreshTokenRepository(verifyContext);

        Assert.False((await verifyRepository.GetByTokenHashAsync("hash-3a"))!.IsActive);
        Assert.False((await verifyRepository.GetByTokenHashAsync("hash-3b"))!.IsActive);
        Assert.True((await verifyRepository.GetByTokenHashAsync("hash-3c"))!.IsActive);
    }
}
