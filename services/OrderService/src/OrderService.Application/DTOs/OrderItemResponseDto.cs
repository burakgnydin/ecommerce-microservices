namespace OrderService.Application.DTOs;

/// <summary>
/// Represents an order line as returned by the API.
/// </summary>
/// <param name="ProductId">Id of the ordered product.</param>
/// <param name="ProductName">Product name, as it was at order time.</param>
/// <param name="UnitPrice">Product price, as it was at order time.</param>
/// <param name="Quantity">Ordered quantity.</param>
/// <param name="Subtotal">UnitPrice multiplied by Quantity.</param>
public record OrderItemResponseDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal Subtotal);
