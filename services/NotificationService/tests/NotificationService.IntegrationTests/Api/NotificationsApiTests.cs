using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NotificationService.Application.DTOs;
using NotificationService.Domain.Enums;
using NotificationService.IntegrationTests.Fixtures;

namespace NotificationService.IntegrationTests.Api;

public class NotificationsApiTests : IClassFixture<NotificationServiceApiFactory>
{
    private readonly NotificationServiceApiFactory _factory;
    private readonly HttpClient _client;

    public NotificationsApiTests(NotificationServiceApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private void Authenticate(Guid userId)
        => _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateAccessToken(userId));

    [Fact]
    public async Task Notify_ReturnsAccepted_WhenRequestIsValid()
    {
        Authenticate(Guid.NewGuid());

        var response = await _client.PostAsJsonAsync("/api/notifications", new NotificationRequestDto(Guid.NewGuid(), NotificationType.OrderCreated));

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }

    [Fact]
    public async Task Notify_ReturnsUnauthorized_WhenNoTokenIsProvided()
    {
        var response = await _client.PostAsJsonAsync("/api/notifications", new NotificationRequestDto(Guid.NewGuid(), NotificationType.OrderCreated));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Notify_ReturnsBadRequest_WhenOrderIdIsEmpty()
    {
        Authenticate(Guid.NewGuid());

        var response = await _client.PostAsJsonAsync("/api/notifications", new NotificationRequestDto(Guid.Empty, NotificationType.OrderCreated));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Notify_ReturnsBadRequest_WhenNotificationTypeIsInvalid()
    {
        Authenticate(Guid.NewGuid());

        var response = await _client.PostAsJsonAsync("/api/notifications", new NotificationRequestDto(Guid.NewGuid(), (NotificationType)999));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
