using ProductService.Domain.Entities;

namespace ProductService.Application.Interfaces;

public interface IStockValidationStrategy
{
    bool CanFulfill(Product product, int requestedQuantity);
}
