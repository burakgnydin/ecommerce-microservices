using OrderService.Application.DTOs;
using OrderService.Domain.Entities;

namespace OrderService.Application.Mapping;

public static class OrderMapper
{
    public static OrderResponseDto ToDto(this Order order)
    {
        return new OrderResponseDto(
            order.Id,
            order.UserId,
            order.Status.ToString(),
            order.TotalAmount,
            order.CreatedAt,
            order.Items.Select(i => i.ToDto()).ToList(),
            order.ShippingTitle,
            order.ShippingCity,
            order.ShippingDistrict,
            order.ShippingFullAddress);
    }

    public static OrderItemResponseDto ToDto(this OrderItem item)
    {
        return new OrderItemResponseDto(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity, item.Subtotal);
    }
}
