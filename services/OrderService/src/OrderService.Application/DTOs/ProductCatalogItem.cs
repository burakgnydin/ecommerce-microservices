namespace OrderService.Application.DTOs;

/// <summary>
/// Product data as seen by order-service, fetched from product-service.
/// </summary>
/// <param name="Id">Product id.</param>
/// <param name="Name">Product name, used as the order item's name snapshot.</param>
/// <param name="Price">Product price at the time of the check.</param>
/// <param name="Stock">Available stock quantity.</param>
public record ProductCatalogItem(
    Guid Id,
    string Name,
    decimal Price,
    int Stock);
