using OrderService.Application.DTOs;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces;
using OrderService.Application.Mapping;
using OrderService.Domain.Entities;

namespace OrderService.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductCatalogClient _productCatalogClient;

    public OrderService(IOrderRepository orderRepository, IProductCatalogClient productCatalogClient)
    {
        _orderRepository = orderRepository;
        _productCatalogClient = productCatalogClient;
    }

    public async Task<OrderResponseDto> CreateAsync(Guid userId, OrderCreateDto dto, CancellationToken cancellationToken = default)
    {
        var items = new List<OrderItem>(dto.Items.Count);

        foreach (var itemRequest in dto.Items)
        {
            var product = await _productCatalogClient.GetProductAsync(itemRequest.ProductId, cancellationToken)
                ?? throw new ProductNotFoundException(itemRequest.ProductId);

            if (product.Stock < itemRequest.Quantity)
            {
                throw new InsufficientStockException(itemRequest.ProductId, itemRequest.Quantity, product.Stock);
            }

            items.Add(new OrderItem(product.Id, product.Name, product.Price, itemRequest.Quantity));
        }

        var order = new Order(userId, items);
        await _orderRepository.CreateAsync(order, cancellationToken);

        return order.ToDto();
    }

    public async Task<OrderResponseDto> GetByIdAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default)
    {
        var order = await GetOwnedOrderAsync(orderId, userId, cancellationToken);
        return order.ToDto();
    }

    public async Task<IReadOnlyList<OrderResponseDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId, cancellationToken);
        return orders.Select(o => o.ToDto()).ToList();
    }

    public async Task<OrderResponseDto> CancelAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default)
    {
        var order = await GetOwnedOrderAsync(orderId, userId, cancellationToken);

        try
        {
            order.Cancel();
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOrderStatusException(ex.Message);
        }

        await _orderRepository.UpdateAsync(order, cancellationToken);
        return order.ToDto();
    }

    // Not-found and not-owned both resolve to the same NotFoundException so a caller cannot
    // distinguish "doesn't exist" from "belongs to someone else" (avoids IDOR enumeration).
    private async Task<Order> GetOwnedOrderAsync(Guid orderId, Guid userId, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order is null || order.UserId != userId)
        {
            throw new NotFoundException($"Order '{orderId}' was not found.");
        }

        return order;
    }
}
