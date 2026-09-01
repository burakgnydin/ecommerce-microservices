using System.Threading.RateLimiting;
using AuthService.Api.ExceptionHandling;
using AuthService.Application.DependencyInjection;
using AuthService.Application.Interfaces;
using AuthService.Application.Validators;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.DependencyInjection;
using AuthService.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Add services to the container.
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
    options.AddPolicy("register", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// Applies pending migrations and, if SeedAdmin:Email/Password are configured, ensures a single
// Admin-role account exists. This is auth-service's only source of Admin users today - registration
// always creates Customer accounts. Idempotent and a no-op when the seed values aren't set.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await dbContext.Database.MigrateAsync();

    var seedEmail = app.Configuration["SeedAdmin:Email"];
    var seedPassword = app.Configuration["SeedAdmin:Password"];
    if (!string.IsNullOrWhiteSpace(seedEmail) && !string.IsNullOrWhiteSpace(seedPassword))
    {
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        if (!await userRepository.ExistsByEmailAsync(seedEmail))
        {
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var admin = new User("Admin", seedEmail, passwordHasher.Hash(seedPassword), Role.Admin);
            await userRepository.CreateAsync(admin);
        }
    }
}

// Configure the HTTP request pipeline.

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// API documentation is only exposed in Development; leaving it public in
// production would expose internal contract details, against our security baseline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();

app.Run();

/// <summary>
/// Exposes the implicit top-level Program class so <c>WebApplicationFactory&lt;Program&gt;</c> can reference it from test assemblies.
/// </summary>
public partial class Program;
