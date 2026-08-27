namespace OrderService.Application.DTOs;

/// <summary>
/// A single line of an order creation request.
/// </summary>
/// <param name="ProductId">Id of the product being ordered.</param>
/// <param name="Quantity">Requested quantity.</param>
public record OrderItemCreateDto(Guid ProductId, int Quantity);
