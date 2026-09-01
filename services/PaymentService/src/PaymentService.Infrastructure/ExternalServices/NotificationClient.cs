using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;

namespace PaymentService.Infrastructure.ExternalServices;

public class NotificationClient : INotificationClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationClient> _logger;

    public NotificationClient(HttpClient httpClient, ILogger<NotificationClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task NotifyAsync(Guid orderId, PaymentNotificationType type, string bearerToken, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/notifications")
            {
                Content = JsonContent.Create(new { OrderId = orderId, Type = type })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "notification-service returned an unexpected status code {StatusCode} while notifying about order {OrderId}.",
                    (int)response.StatusCode, orderId);
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TimeoutException)
        {
            _logger.LogWarning(ex, "notification-service could not be reached while notifying about order {OrderId}.", orderId);
        }
    }
}
