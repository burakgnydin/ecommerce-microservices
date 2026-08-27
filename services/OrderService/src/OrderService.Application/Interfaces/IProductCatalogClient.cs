using OrderService.Application.DTOs;

namespace OrderService.Application.Interfaces;

public interface IProductCatalogClient
{
    /// <summary>
    /// Returns the product, or null if product-service reports it does not exist.
    /// </summary>
    /// <exception cref="Exceptions.ProductServiceUnavailableException">
    /// Thrown when product-service cannot be reached after retries.
    /// </exception>
    Task<ProductCatalogItem?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default);
}
