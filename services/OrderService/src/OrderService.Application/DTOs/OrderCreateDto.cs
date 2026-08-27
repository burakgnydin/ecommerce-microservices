namespace OrderService.Application.DTOs;

/// <summary>
/// Payload used to create a new order.
/// </summary>
/// <param name="Items">Products and quantities to order. Must contain at least one item.</param>
public record OrderCreateDto(IReadOnlyList<OrderItemCreateDto> Items);
