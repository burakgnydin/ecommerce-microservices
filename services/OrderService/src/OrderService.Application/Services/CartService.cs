using OrderService.Application.DTOs;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces;
using OrderService.Application.Mapping;
using OrderService.Domain.Entities;

namespace OrderService.Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IOrderService _orderService;

    public CartService(ICartRepository cartRepository, IOrderService orderService)
    {
        _cartRepository = cartRepository;
        _orderService = orderService;
    }

    public async Task<CartResponseDto> GetOrCreateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        return cart.ToDto();
    }

    public async Task<CartResponseDto> AddItemAsync(Guid userId, CartItemAddDto dto, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        cart.AddItem(dto.ProductId, dto.Quantity);
        await _cartRepository.UpdateAsync(cart, cancellationToken);
        return cart.ToDto();
    }

    public async Task<CartResponseDto> UpdateItemQuantityAsync(Guid userId, Guid productId, CartItemQuantityUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var cart = await GetOwnedCartAsync(userId, cancellationToken);

        try
        {
            cart.UpdateItemQuantity(productId, dto.Quantity);
        }
        catch (InvalidOperationException ex)
        {
            throw new NotFoundException(ex.Message);
        }

        await _cartRepository.UpdateAsync(cart, cancellationToken);
        return cart.ToDto();
    }

    public async Task<CartResponseDto> RemoveItemAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOwnedCartAsync(userId, cancellationToken);
        cart.RemoveItem(productId);
        await _cartRepository.UpdateAsync(cart, cancellationToken);
        return cart.ToDto();
    }

    public async Task<OrderResponseDto> CheckoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOwnedCartAsync(userId, cancellationToken);
        if (cart.Items.Count == 0)
        {
            throw new EmptyCartException("Cart is empty.");
        }

        var orderDto = new OrderCreateDto(
            cart.Items.Select(i => new OrderItemCreateDto(i.ProductId, i.Quantity)).ToList());

        var order = await _orderService.CreateAsync(userId, orderDto, cancellationToken);

        cart.Clear();
        await _cartRepository.UpdateAsync(cart, cancellationToken);

        return order;
    }

    private async Task<Cart> GetOrCreateCartAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId, cancellationToken);
        if (cart is not null)
        {
            return cart;
        }

        cart = new Cart(userId);
        await _cartRepository.CreateAsync(cart, cancellationToken);
        return cart;
    }

    private async Task<Cart> GetOwnedCartAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _cartRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException($"Cart for user '{userId}' was not found.");
    }
}
