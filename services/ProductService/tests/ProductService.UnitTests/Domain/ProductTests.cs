using ProductService.Domain.Entities;

namespace ProductService.UnitTests.Domain;

public class ProductTests
{
    [Fact]
    public void Constructor_CreatesProduct_WhenDataIsValid()
    {
        var product = new Product("Widget", "A useful widget", 9.99m, 10, Guid.NewGuid());

        Assert.Equal("Widget", product.Name);
        Assert.Equal(9.99m, product.Price);
        Assert.Equal(10, product.Stock);
        Assert.NotEqual(Guid.Empty, product.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenNameIsEmptyOrWhitespace(string? name)
    {
        Assert.Throws<ArgumentException>(() => new Product(name!, null, 9.99m, 10, Guid.NewGuid()));
    }

    [Fact]
    public void Constructor_Throws_WhenNameExceedsMaxLength()
    {
        var name = new string('a', 201);

        Assert.Throws<ArgumentException>(() => new Product(name, null, 9.99m, 10, Guid.NewGuid()));
    }

    [Fact]
    public void Constructor_Throws_WhenDescriptionExceedsMaxLength()
    {
        var description = new string('a', 2001);

        Assert.Throws<ArgumentException>(() => new Product("Widget", description, 9.99m, 10, Guid.NewGuid()));
    }

    [Fact]
    public void Constructor_Throws_WhenPriceIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Product("Widget", null, -1m, 10, Guid.NewGuid()));
    }

    [Fact]
    public void Constructor_Throws_WhenStockIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Product("Widget", null, 9.99m, -1, Guid.NewGuid()));
    }

    [Fact]
    public void UpdateDetails_UpdatesFields_WhenDataIsValid()
    {
        var product = new Product("Widget", null, 9.99m, 10, Guid.NewGuid());
        var newCategoryId = Guid.NewGuid();

        product.UpdateDetails("New Name", "New description", 19.99m, 5, newCategoryId, allowsPreOrder: true);

        Assert.Equal("New Name", product.Name);
        Assert.Equal("New description", product.Description);
        Assert.Equal(19.99m, product.Price);
        Assert.Equal(5, product.Stock);
        Assert.Equal(newCategoryId, product.CategoryId);
        Assert.True(product.AllowsPreOrder);
    }

    [Fact]
    public void UpdateDetails_Throws_WhenNameIsEmpty()
    {
        var product = new Product("Widget", null, 9.99m, 10, Guid.NewGuid());

        Assert.Throws<ArgumentException>(() => product.UpdateDetails("", null, 9.99m, 10, Guid.NewGuid(), false));
    }

    [Fact]
    public void UpdateDetails_Throws_WhenPriceIsNegative()
    {
        var product = new Product("Widget", null, 9.99m, 10, Guid.NewGuid());

        Assert.Throws<ArgumentOutOfRangeException>(() => product.UpdateDetails("Widget", null, -1m, 10, Guid.NewGuid(), false));
    }
}
