namespace OrderService.Application.DTOs;

/// <summary>
/// Payload used to set a cart item's quantity.
/// </summary>
/// <param name="Quantity">New quantity for the item.</param>
public record CartItemQuantityUpdateDto(int Quantity);
