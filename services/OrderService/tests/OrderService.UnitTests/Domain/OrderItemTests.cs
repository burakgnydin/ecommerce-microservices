using OrderService.Domain.Entities;

namespace OrderService.UnitTests.Domain;

public class OrderItemTests
{
    [Fact]
    public void Constructor_CreatesOrderItem_WhenDataIsValid()
    {
        var item = new OrderItem(Guid.NewGuid(), "Widget", 9.99m, 3);

        Assert.Equal("Widget", item.ProductName);
        Assert.Equal(9.99m, item.UnitPrice);
        Assert.Equal(3, item.Quantity);
        Assert.Equal(29.97m, item.Subtotal);
        Assert.NotEqual(Guid.Empty, item.Id);
    }

    [Fact]
    public void Constructor_Throws_WhenProductIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new OrderItem(Guid.Empty, "Widget", 9.99m, 1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenProductNameIsEmptyOrWhitespace(string? productName)
    {
        Assert.Throws<ArgumentException>(() => new OrderItem(Guid.NewGuid(), productName!, 9.99m, 1));
    }

    [Fact]
    public void Constructor_Throws_WhenUnitPriceIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new OrderItem(Guid.NewGuid(), "Widget", -1m, 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_Throws_WhenQuantityIsNotPositive(int quantity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new OrderItem(Guid.NewGuid(), "Widget", 9.99m, quantity));
    }
}
