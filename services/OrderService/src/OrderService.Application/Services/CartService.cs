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
    private readonly IProductCatalogClient _productCatalogClient;

    public CartService(ICartRepository cartRepository, IOrderService orderService, IProductCatalogClient productCatalogClient)
    {
        _cartRepository = cartRepository;
        _orderService = orderService;
        _productCatalogClient = productCatalogClient;
    }

    public async Task<CartResponseDto> GetOrCreateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        return cart.ToDto();
    }

    public async Task<CartResponseDto> AddItemAsync(Guid userId, CartItemAddDto dto, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);

        var existingQuantity = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId)?.Quantity ?? 0;
        var requestedTotal = existingQuantity + dto.Quantity;
        await EnsureStockAvailableAsync(dto.ProductId, requestedTotal, cancellationToken);

        cart.AddItem(dto.ProductId, dto.Quantity);
        await _cartRepository.UpdateAsync(cart, cancellationToken);
        return cart.ToDto();
    }

    public async Task<CartResponseDto> UpdateItemQuantityAsync(Guid userId, Guid productId, CartItemQuantityUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var cart = await GetOwnedCartAsync(userId, cancellationToken);

        await EnsureStockAvailableAsync(productId, dto.Quantity, cancellationToken);

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

    private async Task EnsureStockAvailableAsync(Guid productId, int requestedQuantity, CancellationToken cancellationToken)
    {
        var product = await _productCatalogClient.GetProductAsync(productId, cancellationToken)
            ?? throw new ProductNotFoundException(productId);

        if (product.Stock < requestedQuantity)
        {
            throw new InsufficientStockException(productId, requestedQuantity, product.Stock);
        }
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
