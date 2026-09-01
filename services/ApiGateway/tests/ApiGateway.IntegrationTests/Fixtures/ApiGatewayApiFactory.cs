using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ApiGateway.IntegrationTests.Fixtures;

/// <summary>
/// Boots the real gateway pipeline with cluster destinations repointed at a fake downstream
/// server, so routing/prefix-stripping can be verified without the real backend services.
/// </summary>
public class ApiGatewayApiFactory : WebApplicationFactory<Program>
{
    private readonly string _downstreamAddress;

    public ApiGatewayApiFactory(string downstreamAddress)
    {
        _downstreamAddress = downstreamAddress;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ReverseProxy:Clusters:auth-cluster:Destinations:destination1:Address"] = _downstreamAddress,
                ["ReverseProxy:Clusters:products-cluster:Destinations:destination1:Address"] = _downstreamAddress,
                ["ReverseProxy:Clusters:orders-cluster:Destinations:destination1:Address"] = _downstreamAddress,
                ["ReverseProxy:Clusters:payments-cluster:Destinations:destination1:Address"] = _downstreamAddress,
                ["ReverseProxy:Clusters:notifications-cluster:Destinations:destination1:Address"] = _downstreamAddress
            });
        });
    }
}
