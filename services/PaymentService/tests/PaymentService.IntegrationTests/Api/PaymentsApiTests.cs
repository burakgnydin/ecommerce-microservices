using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PaymentService.Application.DTOs;
using PaymentService.IntegrationTests.Fixtures;

namespace PaymentService.IntegrationTests.Api;

[Collection("Integration")]
public class PaymentsApiTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private PaymentServiceApiFactory _factory = null!;
    private HttpClient _client = null!;

    public PaymentsApiTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync()
    {
        _factory = new PaymentServiceApiFactory(_fixture.ConnectionString);
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

    private static PaymentRequestDto CreateRequest(Guid orderId, string cardNumber = "4111111111111234")
        => new(orderId, cardNumber, 12, DateTime.UtcNow.Year + 1, "123");

    [Fact]
    public async Task Charge_ReturnsCreatedPayment_AndMarksOrderAsPaid_WhenOrderIsPendingAndCardIsApproved()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        _factory.OrderServiceHandler.SetOrder(orderId, userId, "Pending", 19.98m);
        Authenticate(userId);

        var response = await _client.PostAsJsonAsync("/api/payments", CreateRequest(orderId));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var payment = await response.Content.ReadFromJsonAsync<PaymentResponseDto>();
        Assert.Equal("Succeeded", payment!.Status);
        Assert.Equal(19.98m, payment.Amount);
        Assert.Contains(orderId, _factory.OrderServiceHandler.MarkedAsPaidOrderIds);
        Assert.Contains((orderId, (int)PaymentNotificationType.PaymentSucceeded), _factory.NotificationServiceHandler.Notifications);
    }

    [Fact]
    public async Task Charge_ReturnsUnauthorized_WhenNoTokenIsProvided()
    {
        var response = await _client.PostAsJsonAsync("/api/payments", CreateRequest(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Charge_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        Authenticate(Guid.NewGuid());

        var response = await _client.PostAsJsonAsync("/api/payments", CreateRequest(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Charge_ReturnsConflict_WhenOrderIsNotPending()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        _factory.OrderServiceHandler.SetOrder(orderId, userId, "Paid", 19.98m);
        Authenticate(userId);

        var response = await _client.PostAsJsonAsync("/api/payments", CreateRequest(orderId));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Charge_ReturnsPaymentRequired_WhenCardIsDeclined()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        _factory.OrderServiceHandler.SetOrder(orderId, userId, "Pending", 19.98m);
        Authenticate(userId);

        var response = await _client.PostAsJsonAsync("/api/payments", CreateRequest(orderId, "4111111111110000"));

        Assert.Equal(HttpStatusCode.PaymentRequired, response.StatusCode);
        Assert.Empty(_factory.OrderServiceHandler.MarkedAsPaidOrderIds);
        Assert.Contains((orderId, (int)PaymentNotificationType.PaymentFailed), _factory.NotificationServiceHandler.Notifications);
    }

    [Fact]
    public async Task Charge_ReturnsBadRequest_WhenCardNumberIsInvalid()
    {
        Authenticate(Guid.NewGuid());

        var response = await _client.PostAsJsonAsync("/api/payments", CreateRequest(Guid.NewGuid(), "123"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
