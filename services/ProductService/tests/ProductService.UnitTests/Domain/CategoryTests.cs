using ProductService.Domain.Entities;

namespace ProductService.UnitTests.Domain;

public class CategoryTests
{
    [Fact]
    public void Constructor_CreatesCategory_WhenNameIsValid()
    {
        var category = new Category("Electronics");

        Assert.Equal("Electronics", category.Name);
        Assert.NotEqual(Guid.Empty, category.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenNameIsEmptyOrWhitespace(string? name)
    {
        Assert.Throws<ArgumentException>(() => new Category(name!));
    }

    [Fact]
    public void Constructor_Throws_WhenNameExceedsMaxLength()
    {
        var name = new string('a', 101);

        Assert.Throws<ArgumentException>(() => new Category(name));
    }
}
