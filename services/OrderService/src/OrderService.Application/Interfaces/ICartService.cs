using OrderService.Application.DTOs;

namespace OrderService.Application.Interfaces;

public interface ICartService
{
    Task<CartResponseDto> GetOrCreateAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<CartResponseDto> AddItemAsync(Guid userId, CartItemAddDto dto, CancellationToken cancellationToken = default);

    Task<CartResponseDto> UpdateItemQuantityAsync(Guid userId, Guid productId, CartItemQuantityUpdateDto dto, CancellationToken cancellationToken = default);

    Task<CartResponseDto> RemoveItemAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);

    Task<OrderResponseDto> CheckoutAsync(Guid userId, CheckoutRequestDto dto, CancellationToken cancellationToken = default);
}
