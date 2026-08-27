using OrderService.Application.DTOs;
using OrderService.Domain.Entities;

namespace OrderService.Application.Mapping;

public static class CartMapper
{
    public static CartResponseDto ToDto(this Cart cart)
    {
        return new CartResponseDto(
            cart.Id,
            cart.UserId,
            cart.Items.Select(i => i.ToDto()).ToList(),
            cart.UpdatedAt);
    }

    public static CartItemResponseDto ToDto(this CartItem item)
    {
        return new CartItemResponseDto(item.ProductId, item.Quantity);
    }
}
