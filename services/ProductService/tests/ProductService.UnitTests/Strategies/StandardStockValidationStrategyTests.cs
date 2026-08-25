using ProductService.Application.Strategies;
using ProductService.Domain.Entities;

namespace ProductService.UnitTests.Strategies;

public class StandardStockValidationStrategyTests
{
    private readonly StandardStockValidationStrategy _strategy = new();

    [Fact]
    public void CanFulfill_ReturnsTrue_WhenRequestedQuantityIsWithinStock()
    {
        var product = new Product("Widget", null, 9.99m, 10, Guid.NewGuid());

        var result = _strategy.CanFulfill(product, 5);

        Assert.True(result);
    }

    [Fact]
    public void CanFulfill_ReturnsTrue_WhenRequestedQuantityEqualsStock()
    {
        var product = new Product("Widget", null, 9.99m, 10, Guid.NewGuid());

        var result = _strategy.CanFulfill(product, 10);

        Assert.True(result);
    }

    [Fact]
    public void CanFulfill_ReturnsFalse_WhenRequestedQuantityExceedsStock()
    {
        var product = new Product("Widget", null, 9.99m, 10, Guid.NewGuid());

        var result = _strategy.CanFulfill(product, 11);

        Assert.False(result);
    }
}
