using System.Net;
using System.Net.Http.Json;

namespace OrderService.IntegrationTests.Fixtures;

/// <summary>
/// Stands in for product-service's HTTP API. Tests register the products a stubbed
/// GET /api/products/{id} call should return, so order creation can be exercised end-to-end
/// without a real product-service instance.
/// </summary>
public class StubProductServiceHandler : HttpMessageHandler
{
    private readonly Dictionary<Guid, (string Name, decimal Price, int Stock)> _products = new();

    public void SetProduct(Guid productId, string name, decimal price, int stock)
        => _products[productId] = (name, price, stock);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var productId = Guid.Parse(request.RequestUri!.Segments[^1]);

        if (!_products.TryGetValue(productId, out var product))
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { Id = productId, product.Name, product.Price, product.Stock })
        };
        return Task.FromResult(response);
    }
}
