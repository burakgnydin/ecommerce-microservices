using Moq;
using OrderService.Application.DTOs;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using SutOrderService = OrderService.Application.Services.OrderService;

namespace OrderService.UnitTests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepository = new();
    private readonly Mock<IProductCatalogClient> _productCatalogClient = new();
    private readonly SutOrderService _sut;

    public OrderServiceTests()
    {
        _sut = new SutOrderService(_orderRepository.Object, _productCatalogClient.Object);
    }

    private static ProductCatalogItem CreateProduct(Guid? id = null, decimal price = 9.99m, int stock = 10)
        => new(id ?? Guid.NewGuid(), "Widget", price, stock);

    [Fact]
    public async Task CreateAsync_CreatesOrder_WhenProductsExistAndStockIsSufficient()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = CreateProduct(productId, price: 9.99m, stock: 10);
        _productCatalogClient.Setup(c => c.GetProductAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        var dto = new OrderCreateDto([new OrderItemCreateDto(productId, 2)]);

        var result = await _sut.CreateAsync(userId, dto);

        Assert.Equal(userId, result.UserId);
        Assert.Equal(19.98m, result.TotalAmount);
        _orderRepository.Verify(r => r.CreateAsync(It.Is<Order>(o => o.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenProductNotFound()
    {
        var productId = Guid.NewGuid();
        _productCatalogClient.Setup(c => c.GetProductAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync((ProductCatalogItem?)null);
        var dto = new OrderCreateDto([new OrderItemCreateDto(productId, 1)]);

        await Assert.ThrowsAsync<ProductNotFoundException>(() => _sut.CreateAsync(Guid.NewGuid(), dto));
        _orderRepository.Verify(r => r.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenStockIsInsufficient()
    {
        var productId = Guid.NewGuid();
        var product = CreateProduct(productId, stock: 1);
        _productCatalogClient.Setup(c => c.GetProductAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        var dto = new OrderCreateDto([new OrderItemCreateDto(productId, 5)]);

        await Assert.ThrowsAsync<InsufficientStockException>(() => _sut.CreateAsync(Guid.NewGuid(), dto));
        _orderRepository.Verify(r => r.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_PropagatesException_WhenProductCatalogIsUnavailable()
    {
        var productId = Guid.NewGuid();
        _productCatalogClient.Setup(c => c.GetProductAsync(productId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ProductServiceUnavailableException("product-service is unreachable."));
        var dto = new OrderCreateDto([new OrderItemCreateDto(productId, 1)]);

        await Assert.ThrowsAsync<ProductServiceUnavailableException>(() => _sut.CreateAsync(Guid.NewGuid(), dto));
        _orderRepository.Verify(r => r.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsOrder_WhenOwnedByUser()
    {
        var userId = Guid.NewGuid();
        var order = new Order(userId, [new OrderItem(Guid.NewGuid(), "Widget", 9.99m, 1)]);
        _orderRepository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _sut.GetByIdAsync(order.Id, userId);

        Assert.Equal(order.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_Throws_WhenOrderDoesNotExist()
    {
        var orderId = Guid.NewGuid();
        _orderRepository.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(orderId, Guid.NewGuid()));
    }

    [Fact]
    public async Task GetByIdAsync_Throws_WhenOrderIsNotOwnedByUser()
    {
        var order = new Order(Guid.NewGuid(), [new OrderItem(Guid.NewGuid(), "Widget", 9.99m, 1)]);
        _orderRepository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(order.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task CancelAsync_CancelsOrder_WhenPendingAndOwnedByUser()
    {
        var userId = Guid.NewGuid();
        var order = new Order(userId, [new OrderItem(Guid.NewGuid(), "Widget", 9.99m, 1)]);
        _orderRepository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _sut.CancelAsync(order.Id, userId);

        Assert.Equal("Cancelled", result.Status);
        _orderRepository.Verify(r => r.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_Throws_WhenOrderIsNotOwnedByUser()
    {
        var order = new Order(Guid.NewGuid(), [new OrderItem(Guid.NewGuid(), "Widget", 9.99m, 1)]);
        _orderRepository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.CancelAsync(order.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task CancelAsync_Throws_WhenOrderIsAlreadyCancelled()
    {
        var userId = Guid.NewGuid();
        var order = new Order(userId, [new OrderItem(Guid.NewGuid(), "Widget", 9.99m, 1)]);
        order.Cancel();
        _orderRepository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        await Assert.ThrowsAsync<InvalidOrderStatusException>(() => _sut.CancelAsync(order.Id, userId));
    }

    [Fact]
    public async Task MarkAsPaidAsync_MarksOrderAsPaid_WhenPendingAndOwnedByUser()
    {
        var userId = Guid.NewGuid();
        var order = new Order(userId, [new OrderItem(Guid.NewGuid(), "Widget", 9.99m, 1)]);
        _orderRepository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _sut.MarkAsPaidAsync(order.Id, userId);

        Assert.Equal("Paid", result.Status);
        _orderRepository.Verify(r => r.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAsPaidAsync_Throws_WhenOrderIsNotOwnedByUser()
    {
        var order = new Order(Guid.NewGuid(), [new OrderItem(Guid.NewGuid(), "Widget", 9.99m, 1)]);
        _orderRepository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.MarkAsPaidAsync(order.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task MarkAsPaidAsync_Throws_WhenOrderIsAlreadyPaid()
    {
        var userId = Guid.NewGuid();
        var order = new Order(userId, [new OrderItem(Guid.NewGuid(), "Widget", 9.99m, 1)]);
        order.MarkAsPaid();
        _orderRepository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        await Assert.ThrowsAsync<InvalidOrderStatusException>(() => _sut.MarkAsPaidAsync(order.Id, userId));
    }
}
