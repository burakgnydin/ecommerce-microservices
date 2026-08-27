namespace OrderService.Infrastructure.ExternalServices;

/// <summary>
/// Mirrors the subset of product-service's ProductResponseDto that order-service needs.
/// </summary>
internal record ProductServiceProductResponse(
    Guid Id,
    string Name,
    decimal Price,
    int Stock);
