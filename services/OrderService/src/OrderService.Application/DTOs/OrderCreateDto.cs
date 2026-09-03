namespace OrderService.Application.DTOs;

/// <summary>
/// Payload used to create a new order.
/// </summary>
/// <param name="Items">Products and quantities to order. Must contain at least one item.</param>
/// <param name="ShippingTitle">User-facing label for the shipping address, e.g. "Ev" or "İş".</param>
/// <param name="ShippingCity">Shipping city (il).</param>
/// <param name="ShippingDistrict">Shipping district (ilçe).</param>
/// <param name="ShippingFullAddress">Full shipping street address.</param>
public record OrderCreateDto(
    IReadOnlyList<OrderItemCreateDto> Items,
    string ShippingTitle,
    string ShippingCity,
    string ShippingDistrict,
    string ShippingFullAddress);
