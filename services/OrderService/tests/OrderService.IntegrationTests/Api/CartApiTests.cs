using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using OrderService.Application.DTOs;
using OrderService.IntegrationTests.Fixtures;

namespace OrderService.IntegrationTests.Api;

[Collection("Integration")]
public class CartApiTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private OrderServiceApiFactory _factory = null!;
    private HttpClient _client = null!;

    public CartApiTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync()
    {
        _factory = new OrderServiceApiFactory(_fixture.ConnectionString);
        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    private void Authenticate(Guid userId)
        => _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateAccessToken(userId));

    [Fact]
    public async Task AddItemThenGet_ReturnsCartWithItem()
    {
        var productId = Guid.NewGuid();
        Authenticate(Guid.NewGuid());

        var addResponse = await _client.PostAsJsonAsync("/api/cart/items", new CartItemAddDto(productId, 3));
        Assert.Equal(HttpStatusCode.OK, addResponse.StatusCode);

        var getResponse = await _client.GetAsync("/api/cart");
        var cart = await getResponse.Content.ReadFromJsonAsync<CartResponseDto>();

        var item = Assert.Single(cart!.Items);
        Assert.Equal(productId, item.ProductId);
        Assert.Equal(3, item.Quantity);
    }

    [Fact]
    public async Task UpdateItemQuantity_UpdatesQuantity()
    {
        var productId = Guid.NewGuid();
        Authenticate(Guid.NewGuid());
        await _client.PostAsJsonAsync("/api/cart/items", new CartItemAddDto(productId, 1));

        var updateResponse = await _client.PutAsJsonAsync($"/api/cart/items/{productId}", new CartItemQuantityUpdateDto(5));

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var cart = await updateResponse.Content.ReadFromJsonAsync<CartResponseDto>();
        Assert.Equal(5, cart!.Items.Single().Quantity);
    }

    [Fact]
    public async Task RemoveItem_RemovesItemFromCart()
    {
        var productId = Guid.NewGuid();
        Authenticate(Guid.NewGuid());
        await _client.PostAsJsonAsync("/api/cart/items", new CartItemAddDto(productId, 1));

        var removeResponse = await _client.DeleteAsync($"/api/cart/items/{productId}");

        Assert.Equal(HttpStatusCode.OK, removeResponse.StatusCode);
        var cart = await removeResponse.Content.ReadFromJsonAsync<CartResponseDto>();
        Assert.Empty(cart!.Items);
    }

    [Fact]
    public async Task Checkout_CreatesOrderAndClearsCart_WhenProductExistsWithSufficientStock()
    {
        var productId = Guid.NewGuid();
        _factory.ProductServiceHandler.SetProduct(productId, "Widget", 9.99m, 10);
        Authenticate(Guid.NewGuid());
        await _client.PostAsJsonAsync("/api/cart/items", new CartItemAddDto(productId, 2));

        var checkoutResponse = await _client.PostAsync("/api/cart/checkout", null);

        Assert.Equal(HttpStatusCode.Created, checkoutResponse.StatusCode);
        var order = await checkoutResponse.Content.ReadFromJsonAsync<OrderResponseDto>();
        Assert.Equal(19.98m, order!.TotalAmount);

        var cartAfterCheckout = await (await _client.GetAsync("/api/cart")).Content.ReadFromJsonAsync<CartResponseDto>();
        Assert.Empty(cartAfterCheckout!.Items);
    }

    [Fact]
    public async Task Checkout_ReturnsBadRequest_WhenCartExistsButIsEmpty()
    {
        Authenticate(Guid.NewGuid());
        await _client.GetAsync("/api/cart");

        var response = await _client.PostAsync("/api/cart/checkout", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Checkout_ReturnsNotFound_WhenCartWasNeverCreated()
    {
        Authenticate(Guid.NewGuid());

        var response = await _client.PostAsync("/api/cart/checkout", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
