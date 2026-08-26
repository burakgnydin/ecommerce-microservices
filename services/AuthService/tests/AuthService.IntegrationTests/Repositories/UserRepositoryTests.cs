using AuthService.Domain.Entities;
using AuthService.IntegrationTests.Fixtures;
using AuthService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AuthService.IntegrationTests.Repositories;

[Collection("Integration")]
public class UserRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public UserRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateAsync_PersistsUser_RetrievableByGetByIdAndGetByEmail()
    {
        await using var context = _fixture.CreateDbContext();
        var repository = new UserRepository(context);
        var user = new User("Jane Doe", $"jane-{Guid.NewGuid()}@example.com", "hashed-password");

        await repository.CreateAsync(user);

        await using var verifyContext = _fixture.CreateDbContext();
        var verifyRepository = new UserRepository(verifyContext);

        var byId = await verifyRepository.GetByIdAsync(user.Id);
        Assert.NotNull(byId);
        Assert.Equal(user.Email, byId!.Email);

        var byEmail = await verifyRepository.GetByEmailAsync(user.Email);
        Assert.NotNull(byEmail);
        Assert.Equal(user.Id, byEmail!.Id);
    }

    [Fact]
    public async Task ExistsByEmailAsync_ReturnsTrue_WhenUserExists_AndFalse_WhenItDoesNot()
    {
        await using var context = _fixture.CreateDbContext();
        var repository = new UserRepository(context);
        var user = new User("Jane Doe", $"jane-{Guid.NewGuid()}@example.com", "hashed-password");
        await repository.CreateAsync(user);

        Assert.True(await repository.ExistsByEmailAsync(user.Email));
        Assert.False(await repository.ExistsByEmailAsync($"missing-{Guid.NewGuid()}@example.com"));
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenEmailAlreadyExists()
    {
        var email = $"jane-{Guid.NewGuid()}@example.com";
        await using var setupContext = _fixture.CreateDbContext();
        await new UserRepository(setupContext).CreateAsync(new User("Jane Doe", email, "hashed-password"));

        await using var duplicateContext = _fixture.CreateDbContext();
        var duplicateRepository = new UserRepository(duplicateContext);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            duplicateRepository.CreateAsync(new User("Jane Impostor", email, "hashed-password")));
    }
}
