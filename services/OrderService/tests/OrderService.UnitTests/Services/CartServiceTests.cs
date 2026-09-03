using Moq;
using OrderService.Application.DTOs;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using SutCartService = OrderService.Application.Services.CartService;

namespace OrderService.UnitTests.Services;

public class CartServiceTests
{
    private readonly Mock<ICartRepository> _cartRepository = new();
    private readonly Mock<IOrderService> _orderService = new();
    private readonly Mock<IProductCatalogClient> _productCatalogClient = new();
    private readonly SutCartService _sut;

    public CartServiceTests()
    {
        _sut = new SutCartService(_cartRepository.Object, _orderService.Object, _productCatalogClient.Object);
    }

    private void SetUpProduct(Guid productId, int stock)
    {
        _productCatalogClient
            .Setup(c => c.GetProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductCatalogItem(productId, "Test Product", 9.99m, stock));
    }

    private static CheckoutRequestDto CreateCheckoutRequest()
        => new("Ev", "İstanbul", "Kadıköy", "Örnek Mah. Örnek Sok. No:1");

    [Fact]
    public async Task GetOrCreateAsync_ReturnsExistingCart_WhenCartAlreadyExists()
    {
        var userId = Guid.NewGuid();
        var cart = new Cart(userId);
        _cartRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);

        var result = await _sut.GetOrCreateAsync(userId);

        Assert.Equal(cart.Id, result.Id);
        _cartRepository.Verify(r => r.CreateAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateAsync_CreatesNewCart_WhenNoneExists()
    {
        var userId = Guid.NewGuid();
        _cartRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((Cart?)null);

        var result = await _sut.GetOrCreateAsync(userId);

        Assert.Equal(userId, result.UserId);
        _cartRepository.Verify(r => r.CreateAsync(It.Is<Cart>(c => c.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_AddsItem_WhenCartAlreadyExists()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cart = new Cart(userId);
        _cartRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        SetUpProduct(productId, stock: 10);

        var result = await _sut.AddItemAsync(userId, new CartItemAddDto(productId, 2));

        var item = Assert.Single(result.Items);
        Assert.Equal(productId, item.ProductId);
        Assert.Equal(2, item.Quantity);
        _cartRepository.Verify(r => r.UpdateAsync(cart, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_UpsertsQuantity_WhenProductAlreadyInCart()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cart = new Cart(userId);
        cart.AddItem(productId, 1);
        _cartRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        SetUpProduct(productId, stock: 10);

        var result = await _sut.AddItemAsync(userId, new CartItemAddDto(productId, 2));

        var item = Assert.Single(result.Items);
        Assert.Equal(3, item.Quantity);
    }

    [Fact]
    public async Task AddItemAsync_Throws_WhenRequestedQuantityExceedsStock()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cart = new Cart(userId);
        _cartRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        SetUpProduct(productId, stock: 50);

        await Assert.ThrowsAsync<InsufficientStockException>(
            () => _sut.AddItemAsync(userId, new CartItemAddDto(productId, 60)));
        _cartRepository.Verify(r => r.UpdateAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddItemAsync_Throws_WhenCumulativeQuantityExceedsStock()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cart = new Cart(userId);
        cart.AddItem(productId, 45);
        _cartRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        SetUpProduct(productId, stock: 50);

        await Assert.ThrowsAsync<InsufficientStockException>(
            () => _sut.AddItemAsync(userId, new CartItemAddDto(productId, 10)));
    }

    [Fact]
    public async Task AddItemAsync_Throws_WhenProductDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cart = new Cart(userId);
        _cartRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        _productCatalogClient
            .Setup(c => c.GetProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductCatalogItem?)null);

        await Assert.ThrowsAsync<ProductNotFoundException>(
            () => _sut.AddItemAsync(userId, new CartItemAddDto(productId, 1)));
    }

    [Fact]
    public async Task UpdateItemQuantityAsync_UpdatesQuantity_WhenItemExists()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cart = new Cart(userId);
        cart.AddItem(productId, 1);
        _cartRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        SetUpProduct(productId, stock: 10);

        var result = await _sut.UpdateItemQuantityAsync(userId, productId, new CartItemQuantityUpdateDto(5));

        Assert.Equal(5, result.Items.Single().Quantity);
    }

    [Fact]
    public async Task UpdateItemQuantityAsync_Throws_WhenQuantityExceedsStock()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cart = new Cart(userId);
        cart.AddItem(productId, 1);
        _cartRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        SetUpProduct(productId, stock: 4);

        await Assert.ThrowsAsync<InsufficientStockException>(
            () => _sut.UpdateItemQuantityAsync(userId, productId, new CartItemQuantityUpdateDto(5)));
    }

    [Fact]
    public async Task UpdateItemQuantityAsync_Throws_WhenCartDoesNotExist()
    {
        var userId = Guid.NewGuid();
        _cartRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((Cart?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.UpdateItemQuantityAsync(userId, Guid.NewGuid(), new CartItemQuantityUpdateDto(1)));
    }

    [Fact]
    public async Task UpdateItemQuantityAsync_Throws_WhenItemNotInCart()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cart = new Cart(userId);
        _cartRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        SetUpProduct(productId, stock: 10);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.UpdateItemQuantityAsync(userId, productId, new CartItemQuantityUpdateDto(1)));
    }

    [Fact]
    public async Task RemoveItemAsync_RemovesItem_WhenCartExists()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cart = new Cart(userId);
        cart.AddItem(productId, 1);
        _cartRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);

        var result = await _sut.RemoveItemAsync(userId, productId);

        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task RemoveItemAsync_Throws_WhenCartDoesNotExist()
    {
        var userId = Guid.NewGuid();
        _cartRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((Cart?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.RemoveItemAsync(userId, Guid.NewGuid()));
    }

    [Fact]
    public async Task CheckoutAsync_CreatesOrderAndClearsCart_WhenCartHasItems()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cart = new Cart(userId);
        cart.AddItem(productId, 2);
        _cartRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        var orderResponse = new OrderResponseDto(Guid.NewGuid(), userId, "Pending", 19.98m, DateTime.UtcNow, [], "Ev", "İstanbul", "Kadıköy", "Örnek Mah. Örnek Sok. No:1");
        _orderService.Setup(s => s.CreateAsync(userId, It.IsAny<OrderCreateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderResponse);

        var result = await _sut.CheckoutAsync(userId, CreateCheckoutRequest());

        Assert.Equal(orderResponse.Id, result.Id);
        Assert.Empty(cart.Items);
        _cartRepository.Verify(r => r.UpdateAsync(cart, It.IsAny<CancellationToken>()), Times.Once);
        _orderService.Verify(s => s.CreateAsync(
            userId,
            It.Is<OrderCreateDto>(d => d.Items.Count == 1 && d.Items[0].ProductId == productId && d.Items[0].Quantity == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckoutAsync_Throws_WhenCartIsEmpty()
    {
        var userId = Guid.NewGuid();
        var cart = new Cart(userId);
        _cartRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);

        await Assert.ThrowsAsync<EmptyCartException>(() => _sut.CheckoutAsync(userId, CreateCheckoutRequest()));
        _orderService.Verify(s => s.CreateAsync(It.IsAny<Guid>(), It.IsAny<OrderCreateDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckoutAsync_Throws_WhenCartDoesNotExist()
    {
        var userId = Guid.NewGuid();
        _cartRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((Cart?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.CheckoutAsync(userId, CreateCheckoutRequest()));
    }
}
