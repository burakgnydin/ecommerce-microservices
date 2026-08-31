using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ApiGateway.IntegrationTests.Fixtures;

/// <summary>
/// Minimal real HTTP server standing in for a downstream service, since YARP proxies over
/// actual sockets and can't be pointed at an in-memory TestServer. Echoes back the path it
/// received so tests can assert on the gateway's prefix-stripping behavior.
/// </summary>
public sealed class FakeDownstreamServer : IAsyncDisposable
{
    private readonly WebApplication _app;

    public string BaseAddress { get; }

    private FakeDownstreamServer(WebApplication app, string baseAddress)
    {
        _app = app;
        BaseAddress = baseAddress;
    }

    public static async Task<FakeDownstreamServer> StartAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();

        var app = builder.Build();
        app.MapGet("/{**catchAll}", (string? catchAll) => Results.Text(catchAll ?? string.Empty));

        await app.StartAsync();
        var baseAddress = app.Urls.First();

        return new FakeDownstreamServer(app, baseAddress);
    }

    public async ValueTask DisposeAsync() => await _app.DisposeAsync();
}
