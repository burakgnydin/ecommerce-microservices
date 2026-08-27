namespace OrderService.Application.DTOs;

/// <summary>
/// A single line of a cart.
/// </summary>
/// <param name="ProductId">Id of the product in the cart.</param>
/// <param name="Quantity">Requested quantity.</param>
public record CartItemResponseDto(Guid ProductId, int Quantity);
