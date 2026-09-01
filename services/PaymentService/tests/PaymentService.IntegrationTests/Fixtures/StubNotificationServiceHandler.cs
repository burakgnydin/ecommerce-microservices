using System.Net;
using System.Net.Http.Json;

namespace PaymentService.IntegrationTests.Fixtures;

/// <summary>
/// Stands in for notification-service's HTTP API. Records every POST /api/notifications
/// call so tests can assert a payment outcome was notified, without a real notification-service instance.
/// </summary>
public class StubNotificationServiceHandler : HttpMessageHandler
{
    public List<(Guid OrderId, int Type)> Notifications { get; } = [];

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = await request.Content!.ReadFromJsonAsync<NotifyRequestBody>(cancellationToken: cancellationToken);
        Notifications.Add((body!.OrderId, body.Type));
        return new HttpResponseMessage(HttpStatusCode.Accepted);
    }

    private record NotifyRequestBody(Guid OrderId, int Type);
}
