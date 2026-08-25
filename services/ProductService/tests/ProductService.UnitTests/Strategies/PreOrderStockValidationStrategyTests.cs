using ProductService.Application.Strategies;
using ProductService.Domain.Entities;

namespace ProductService.UnitTests.Strategies;

public class PreOrderStockValidationStrategyTests
{
    private readonly PreOrderStockValidationStrategy _strategy = new();

    [Fact]
    public void CanFulfill_ReturnsTrue_WhenRequestedQuantityIsWithinStock()
    {
        var product = new Product("Widget", null, 9.99m, 5, Guid.NewGuid(), allowsPreOrder: true);

        var result = _strategy.CanFulfill(product, 5);

        Assert.True(result);
    }

    [Fact]
    public void CanFulfill_ReturnsTrue_WhenResultingStockIsWithinNegativeLimit()
    {
        var product = new Product("Widget", null, 9.99m, 5, Guid.NewGuid(), allowsPreOrder: true);

        var result = _strategy.CanFulfill(product, 15);

        Assert.True(result);
    }

    [Fact]
    public void CanFulfill_ReturnsFalse_WhenResultingStockExceedsNegativeLimit()
    {
        var product = new Product("Widget", null, 9.99m, 5, Guid.NewGuid(), allowsPreOrder: true);

        var result = _strategy.CanFulfill(product, 16);

        Assert.False(result);
    }

    [Fact]
    public void CanFulfill_UsesCustomNegativeLimit_WhenProvided()
    {
        var strategy = new PreOrderStockValidationStrategy(maxNegativeStock: -3);
        var product = new Product("Widget", null, 9.99m, 5, Guid.NewGuid(), allowsPreOrder: true);

        Assert.True(strategy.CanFulfill(product, 8));
        Assert.False(strategy.CanFulfill(product, 9));
    }
}
