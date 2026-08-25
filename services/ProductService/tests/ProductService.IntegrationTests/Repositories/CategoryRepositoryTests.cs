using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Persistence.Repositories;
using ProductService.IntegrationTests.Fixtures;

namespace ProductService.IntegrationTests.Repositories;

[Collection("Integration")]
public class CategoryRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public CategoryRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCategory_WhenItExists()
    {
        await using var setupContext = _fixture.CreateDbContext();
        var category = new Category($"Category-{Guid.NewGuid()}");
        setupContext.Categories.Add(category);
        await setupContext.SaveChangesAsync();

        await using var context = _fixture.CreateDbContext();
        var retrieved = await new CategoryRepository(context).GetByIdAsync(category.Id);

        Assert.NotNull(retrieved);
        Assert.Equal(category.Name, retrieved!.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenCategoryDoesNotExist()
    {
        await using var context = _fixture.CreateDbContext();

        var retrieved = await new CategoryRepository(context).GetByIdAsync(Guid.NewGuid());

        Assert.Null(retrieved);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenCategoryExists_AndFalse_WhenItDoesNot()
    {
        await using var setupContext = _fixture.CreateDbContext();
        var category = new Category($"Category-{Guid.NewGuid()}");
        setupContext.Categories.Add(category);
        await setupContext.SaveChangesAsync();

        await using var context = _fixture.CreateDbContext();
        var repository = new CategoryRepository(context);

        Assert.True(await repository.ExistsAsync(category.Id));
        Assert.False(await repository.ExistsAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ExistsByNameAsync_ReturnsTrue_WhenNameExists_AndFalse_WhenItDoesNot()
    {
        var name = $"Category-{Guid.NewGuid()}";
        await using var setupContext = _fixture.CreateDbContext();
        setupContext.Categories.Add(new Category(name));
        await setupContext.SaveChangesAsync();

        await using var context = _fixture.CreateDbContext();
        var repository = new CategoryRepository(context);

        Assert.True(await repository.ExistsByNameAsync(name));
        Assert.False(await repository.ExistsByNameAsync($"Category-{Guid.NewGuid()}"));
    }

    [Fact]
    public async Task CreateAsync_ThrowsDbUpdateException_WhenNameAlreadyExists()
    {
        var name = $"Category-{Guid.NewGuid()}";
        await using var setupContext = _fixture.CreateDbContext();
        await new CategoryRepository(setupContext).CreateAsync(new Category(name));

        await using var context = _fixture.CreateDbContext();
        var repository = new CategoryRepository(context);

        await Assert.ThrowsAsync<DbUpdateException>(() => repository.CreateAsync(new Category(name)));
    }
}
