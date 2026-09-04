const string FrontendCorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);

// Render's private-network service references (fromService/hostport) resolve to "host:port"
// with no scheme, but YARP's cluster Address needs a full URI. Local/docker-compose config
// already includes "http://", so those are left untouched.
var reverseProxyOverrides = new Dictionary<string, string?>();
foreach (var destination in builder.Configuration.GetSection("ReverseProxy:Clusters").GetChildren()
             .SelectMany(cluster => cluster.GetSection("Destinations").GetChildren()))
{
    var addressKey = $"{destination.Path}:Address";
    var address = builder.Configuration[addressKey];
    if (address is not null && !address.Contains("://"))
    {
        reverseProxyOverrides[addressKey] = $"http://{address}";
    }
}
builder.Configuration.AddInMemoryCollection(reverseProxyOverrides);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    // Gateway only returns JSON (reverse-proxied API responses), never renders HTML/scripts.
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'none'");
    await next();
});

app.UseCors(FrontendCorsPolicy);

app.MapReverseProxy();

app.Run();

/// <summary>
/// Exposes the implicit top-level Program class so <c>WebApplicationFactory&lt;Program&gt;</c> can reference it from test assemblies.
/// </summary>
public partial class Program;
