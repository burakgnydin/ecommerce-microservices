namespace OrderService.Application.DTOs;

/// <summary>
/// Payload used to convert the authenticated user's cart into an order.
/// </summary>
/// <param name="ShippingTitle">User-facing label for the shipping address, e.g. "Ev" or "İş".</param>
/// <param name="ShippingCity">Shipping city (il).</param>
/// <param name="ShippingDistrict">Shipping district (ilçe).</param>
/// <param name="ShippingFullAddress">Full shipping street address.</param>
public record CheckoutRequestDto(
    string ShippingTitle,
    string ShippingCity,
    string ShippingDistrict,
    string ShippingFullAddress);
