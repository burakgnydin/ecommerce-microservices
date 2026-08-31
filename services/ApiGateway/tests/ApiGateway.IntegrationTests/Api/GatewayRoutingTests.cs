using System.Net;
using ApiGateway.IntegrationTests.Fixtures;

namespace ApiGateway.IntegrationTests.Api;

public class GatewayRoutingTests : IAsyncLifetime
{
    private FakeDownstreamServer _downstream = null!;
    private ApiGatewayApiFactory _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _downstream = await FakeDownstreamServer.StartAsync();
        _factory = new ApiGatewayApiFactory(_downstream.BaseAddress);
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _downstream.DisposeAsync();
    }

    [Theory]
    [InlineData("/auth/api/auth/login", "api/auth/login")]
    [InlineData("/products/api/products/1", "api/products/1")]
    [InlineData("/orders/api/orders", "api/orders")]
    public async Task Route_ForwardsToCluster_WithPrefixStripped(string requestPath, string expectedForwardedPath)
    {
        var response = await _client.GetAsync(requestPath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expectedForwardedPath, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task UnmatchedRoute_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/unknown");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
