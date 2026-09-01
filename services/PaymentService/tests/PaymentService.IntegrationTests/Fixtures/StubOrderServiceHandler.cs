using System.Net;
using System.Net.Http.Json;

namespace PaymentService.IntegrationTests.Fixtures;

/// <summary>
/// Stands in for order-service's HTTP API. Tests register the order a stubbed
/// GET /api/orders/{id} call should return, so ChargeAsync's order lookup and Paid transition
/// can be exercised end-to-end without a real order-service instance.
/// </summary>
public class StubOrderServiceHandler : HttpMessageHandler
{
    private readonly Dictionary<Guid, (Guid UserId, string Status, decimal TotalAmount)> _orders = new();

    public List<Guid> MarkedAsPaidOrderIds { get; } = [];

    public void SetOrder(Guid orderId, Guid userId, string status, decimal totalAmount)
        => _orders[orderId] = (userId, status, totalAmount);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method == HttpMethod.Post && request.RequestUri!.Segments[^1] == "mark-paid")
        {
            var markPaidOrderId = Guid.Parse(request.RequestUri.Segments[^2].TrimEnd('/'));
            MarkedAsPaidOrderIds.Add(markPaidOrderId);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }

        var orderId = Guid.Parse(request.RequestUri!.Segments[^1]);

        if (!_orders.TryGetValue(orderId, out var order))
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { Id = orderId, order.UserId, order.Status, order.TotalAmount })
        };
        return Task.FromResult(response);
    }
}
