namespace OrderService.Application.DTOs;

/// <summary>
/// Payload used to add a product to the cart, or increase its quantity if already present.
/// </summary>
/// <param name="ProductId">Id of the product to add.</param>
/// <param name="Quantity">Quantity to add.</param>
public record CartItemAddDto(Guid ProductId, int Quantity);
