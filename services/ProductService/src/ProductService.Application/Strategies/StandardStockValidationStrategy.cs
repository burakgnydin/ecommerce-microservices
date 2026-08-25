using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.Application.Strategies;

public class StandardStockValidationStrategy : IStockValidationStrategy
{
    public bool CanFulfill(Product product, int requestedQuantity)
    {
        return requestedQuantity <= product.Stock;
    }
}
