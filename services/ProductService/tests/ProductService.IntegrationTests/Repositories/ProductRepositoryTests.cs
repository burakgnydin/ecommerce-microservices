using ProductService.Domain.Entities;
using ProductService.Infrastructure.Persistence.Repositories;
using ProductService.IntegrationTests.Fixtures;

namespace ProductService.IntegrationTests.Repositories;

[Collection("Integration")]
public class ProductRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public ProductRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<Category> CreateCategoryAsync()
    {
        await using var context = _fixture.CreateDbContext();
        var category = new Category($"Category-{Guid.NewGuid()}");
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        return category;
    }

    [Fact]
    public async Task CreateAsync_PersistsProduct_RetrievableByGetByIdAsync()
    {
        var category = await CreateCategoryAsync();
        await using var context = _fixture.CreateDbContext();
        var repository = new ProductRepository(context);
        var product = new Product("Widget", "desc", 9.99m, 10, category.Id);

        await repository.CreateAsync(product);

        await using var verifyContext = _fixture.CreateDbContext();
        var retrieved = await new ProductRepository(verifyContext).GetByIdAsync(product.Id);

        Assert.NotNull(retrieved);
        Assert.Equal("Widget", retrieved!.Name);
        Assert.Equal(category.Id, retrieved.CategoryId);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var category = await CreateCategoryAsync();
        await using var setupContext = _fixture.CreateDbContext();
        var product = new Product("Widget", null, 9.99m, 10, category.Id);
        await new ProductRepository(setupContext).CreateAsync(product);

        await using var updateContext = _fixture.CreateDbContext();
        var updateRepository = new ProductRepository(updateContext);
        var toUpdate = await updateRepository.GetByIdAsync(product.Id);
        toUpdate!.UpdateDetails("Updated", "Updated desc", 19.99m, 3, category.Id, allowsPreOrder: true);
        await updateRepository.UpdateAsync(toUpdate);

        await using var verifyContext = _fixture.CreateDbContext();
        var retrieved = await new ProductRepository(verifyContext).GetByIdAsync(product.Id);

        Assert.Equal("Updated", retrieved!.Name);
        Assert.Equal(19.99m, retrieved.Price);
        Assert.True(retrieved.AllowsPreOrder);
    }

    [Fact]
    public async Task DeleteAsync_RemovesProduct()
    {
        var category = await CreateCategoryAsync();
        await using var setupContext = _fixture.CreateDbContext();
        var product = new Product("Widget", null, 9.99m, 10, category.Id);
        await new ProductRepository(setupContext).CreateAsync(product);

        await using var deleteContext = _fixture.CreateDbContext();
        await new ProductRepository(deleteContext).DeleteAsync(product.Id);

        await using var verifyContext = _fixture.CreateDbContext();
        var retrieved = await new ProductRepository(verifyContext).GetByIdAsync(product.Id);

        Assert.Null(retrieved);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenProductExists_AndFalse_WhenItDoesNot()
    {
        var category = await CreateCategoryAsync();
        await using var context = _fixture.CreateDbContext();
        var repository = new ProductRepository(context);
        var product = new Product("Widget", null, 9.99m, 10, category.Id);
        await repository.CreateAsync(product);

        Assert.True(await repository.ExistsAsync(product.Id));
        Assert.False(await repository.ExistsAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedProducts_IncludingCreatedOnes()
    {
        var category = await CreateCategoryAsync();
        await using var context = _fixture.CreateDbContext();
        var repository = new ProductRepository(context);
        var createdIds = new List<Guid>();
        for (var i = 0; i < 3; i++)
        {
            var product = new Product($"Widget-{Guid.NewGuid()}", null, 9.99m, 10, category.Id);
            await repository.CreateAsync(product);
            createdIds.Add(product.Id);
        }

        await using var verifyContext = _fixture.CreateDbContext();
        var page = await new ProductRepository(verifyContext).GetAllAsync(pageNumber: 1, pageSize: 100);

        Assert.True(page.TotalCount >= 3);
        Assert.All(createdIds, id => Assert.Contains(page.Items, p => p.Id == id));
    }
}
