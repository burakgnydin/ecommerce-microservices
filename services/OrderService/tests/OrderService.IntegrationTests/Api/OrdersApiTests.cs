using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using OrderService.Application.DTOs;
using OrderService.IntegrationTests.Fixtures;

namespace OrderService.IntegrationTests.Api;

[Collection("Integration")]
public class OrdersApiTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private OrderServiceApiFactory _factory = null!;
    private HttpClient _client = null!;

    public OrdersApiTests(DatabaseFixture fixture)
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
    public async Task CreateThenGet_ReturnsCreatedOrder_WhenProductExistsWithSufficientStock()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        _factory.ProductServiceHandler.SetProduct(productId, "Widget", 9.99m, 10);
        Authenticate(userId);
        var createDto = new OrderCreateDto([new OrderItemCreateDto(productId, 2)]);

        var createResponse = await _client.PostAsJsonAsync("/api/orders", createDto);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<OrderResponseDto>();
        Assert.NotNull(created);
        Assert.Equal(19.98m, created!.TotalAmount);

        var getResponse = await _client.GetAsync($"/api/orders/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<OrderResponseDto>();
        Assert.Equal(created.Id, fetched!.Id);
    }

    [Fact]
    public async Task Create_ReturnsUnauthorized_WhenNoTokenIsProvided()
    {
        var dto = new OrderCreateDto([new OrderItemCreateDto(Guid.NewGuid(), 1)]);

        var response = await _client.PostAsJsonAsync("/api/orders", dto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsNotFound_WhenProductDoesNotExistInProductService()
    {
        Authenticate(Guid.NewGuid());
        var dto = new OrderCreateDto([new OrderItemCreateDto(Guid.NewGuid(), 1)]);

        var response = await _client.PostAsJsonAsync("/api/orders", dto);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenStockIsInsufficient()
    {
        var productId = Guid.NewGuid();
        _factory.ProductServiceHandler.SetProduct(productId, "Widget", 9.99m, 1);
        Authenticate(Guid.NewGuid());
        var dto = new OrderCreateDto([new OrderItemCreateDto(productId, 5)]);

        var response = await _client.PostAsJsonAsync("/api/orders", dto);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenOrderBelongsToAnotherUser()
    {
        var ownerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        _factory.ProductServiceHandler.SetProduct(productId, "Widget", 9.99m, 10);
        Authenticate(ownerId);
        var createResponse = await _client.PostAsJsonAsync("/api/orders", new OrderCreateDto([new OrderItemCreateDto(productId, 1)]));
        var created = await createResponse.Content.ReadFromJsonAsync<OrderResponseDto>();

        Authenticate(Guid.NewGuid());
        var response = await _client.GetAsync($"/api/orders/{created!.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Cancel_SetsOrderStatusToCancelled()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        _factory.ProductServiceHandler.SetProduct(productId, "Widget", 9.99m, 10);
        Authenticate(userId);
        var createResponse = await _client.PostAsJsonAsync("/api/orders", new OrderCreateDto([new OrderItemCreateDto(productId, 1)]));
        var created = await createResponse.Content.ReadFromJsonAsync<OrderResponseDto>();

        var cancelResponse = await _client.PatchAsJsonAsync($"/api/orders/{created!.Id}", new OrderStatusUpdateDto("Cancelled"));

        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);
        var cancelled = await cancelResponse.Content.ReadFromJsonAsync<OrderResponseDto>();
        Assert.Equal("Cancelled", cancelled!.Status);
    }

    [Fact]
    public async Task UpdateStatus_RejectsPaid_EvenForOrderOwner()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        _factory.ProductServiceHandler.SetProduct(productId, "Widget", 9.99m, 10);
        Authenticate(userId);
        var createResponse = await _client.PostAsJsonAsync("/api/orders", new OrderCreateDto([new OrderItemCreateDto(productId, 1)]));
        var created = await createResponse.Content.ReadFromJsonAsync<OrderResponseDto>();

        var response = await _client.PatchAsJsonAsync($"/api/orders/{created!.Id}", new OrderStatusUpdateDto("Paid"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task MarkAsPaid_SetsOrderStatusToPaid_WhenCalledWithServiceRole()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        _factory.ProductServiceHandler.SetProduct(productId, "Widget", 9.99m, 10);
        Authenticate(userId);
        var createResponse = await _client.PostAsJsonAsync("/api/orders", new OrderCreateDto([new OrderItemCreateDto(productId, 1)]));
        var created = await createResponse.Content.ReadFromJsonAsync<OrderResponseDto>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateServiceToken());
        var response = await _client.PostAsync($"/api/orders/{created!.Id}/mark-paid", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paid = await response.Content.ReadFromJsonAsync<OrderResponseDto>();
        Assert.Equal("Paid", paid!.Status);
    }

    [Fact]
    public async Task MarkAsPaid_ReturnsForbidden_WhenCallerIsRegularUser()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        _factory.ProductServiceHandler.SetProduct(productId, "Widget", 9.99m, 10);
        Authenticate(userId);
        var createResponse = await _client.PostAsJsonAsync("/api/orders", new OrderCreateDto([new OrderItemCreateDto(productId, 1)]));
        var created = await createResponse.Content.ReadFromJsonAsync<OrderResponseDto>();

        var response = await _client.PostAsync($"/api/orders/{created!.Id}/mark-paid", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
