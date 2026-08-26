using System.Security.Cryptography;
using System.Threading.RateLimiting;
using AuthService.Api.ExceptionHandling;
using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Application.Validators;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Persistence.Repositories;
using AuthService.Infrastructure.Security;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using AppAuthService = AuthService.Application.Services.AuthService;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Add services to the container.
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AuthDb")));

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

var jwtSigningRsa = RSA.Create();
jwtSigningRsa.ImportFromPem(jwtOptions.PrivateKeyPem);

// Built once and reused for the app's lifetime: importing a fresh RSA per signing call let
// Microsoft.IdentityModel.Tokens' internal signature-provider cache (keyed by key content, not
// object identity) hand back a provider still bound to an earlier, already-disposed RSA instance.
builder.Services.AddSingleton(new SigningCredentials(new RsaSecurityKey(jwtSigningRsa), SecurityAlgorithms.RsaSha256));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new RsaSecurityKey(jwtSigningRsa),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AppAuthService>();
builder.Services.AddScoped<IUserService, UserService>();

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
