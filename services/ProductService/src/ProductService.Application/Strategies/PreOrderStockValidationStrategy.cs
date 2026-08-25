using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.Application.Strategies;

public class PreOrderStockValidationStrategy : IStockValidationStrategy
{
    private const int DefaultMaxNegativeStock = -10;

    private readonly int _maxNegativeStock;

    public PreOrderStockValidationStrategy(int maxNegativeStock = DefaultMaxNegativeStock)
    {
        _maxNegativeStock = maxNegativeStock;
    }

    public bool CanFulfill(Product product, int requestedQuantity)
    {
        return product.Stock - requestedQuantity >= _maxNegativeStock;
    }
}
