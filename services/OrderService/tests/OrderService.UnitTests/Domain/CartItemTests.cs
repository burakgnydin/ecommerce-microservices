using OrderService.Domain.Entities;

namespace OrderService.UnitTests.Domain;

public class CartItemTests
{
    [Fact]
    public void Constructor_CreatesCartItem_WhenDataIsValid()
    {
        var productId = Guid.NewGuid();

        var item = new CartItem(productId, 3);

        Assert.Equal(productId, item.ProductId);
        Assert.Equal(3, item.Quantity);
        Assert.NotEqual(Guid.Empty, item.Id);
    }

    [Fact]
    public void Constructor_Throws_WhenProductIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new CartItem(Guid.Empty, 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_Throws_WhenQuantityIsNotPositive(int quantity)
    {
        Assert.Throws<ArgumentException>(() => new CartItem(Guid.NewGuid(), quantity));
    }
}
