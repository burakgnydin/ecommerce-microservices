using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AuthService.Application.DTOs;
using AuthService.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AuthService.IntegrationTests.Api;

[Collection("Integration")]
public class AuthApiTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly DatabaseFixture _fixture;
    private AuthServiceApiFactory _factory = null!;
    private HttpClient _client = null!;

    public AuthApiTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync()
    {
        _factory = new AuthServiceApiFactory(_fixture.ConnectionString);
        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    private async Task<TokenResponseDto> RegisterThenLoginAsync(string email, string password = "P@ssw0rd!")
    {
        var registerDto = new RegisterRequestDto("Jane Doe", email, password);
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginDto = new LoginRequestDto(email, password);
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var tokens = await loginResponse.Content.ReadFromJsonAsync<TokenResponseDto>();
        Assert.NotNull(tokens);
        return tokens!;
    }

    private static string GetRefreshTokenCookieValue(HttpResponseMessage response)
    {
        var setCookieHeader = response.Headers.GetValues("Set-Cookie").Single(h => h.StartsWith("refreshToken="));
        var cookiePair = setCookieHeader.Split(';')[0];
        return cookiePair["refreshToken=".Length..];
    }

    private static HttpRequestMessage CreateRequestWithRefreshCookie(string path, string refreshTokenCookieValue)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path);
        request.Headers.Add("Cookie", $"refreshToken={refreshTokenCookieValue}");
        return request;
    }

    [Fact]
    public async Task Register_ReturnsConflict_WhenEmailAlreadyExists()
    {
        var email = $"jane-{Guid.NewGuid()}@example.com";
        await RegisterThenLoginAsync(email);

        var duplicateResponse = await _client.PostAsJsonAsync(
            "/api/auth/register", new RegisterRequestDto("Jane Impostor", email, "P@ssw0rd!"));

        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task Register_ReturnsTooManyRequests_AfterExceedingRateLimit()
    {
        for (var i = 0; i < 5; i++)
        {
            var email = $"jane-{Guid.NewGuid()}@example.com";
            var response = await _client.PostAsJsonAsync(
                "/api/auth/register", new RegisterRequestDto("Jane Doe", email, "P@ssw0rd!"));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        var throttledResponse = await _client.PostAsJsonAsync(
            "/api/auth/register", new RegisterRequestDto("Jane Doe", $"jane-{Guid.NewGuid()}@example.com", "P@ssw0rd!"));

        Assert.Equal(HttpStatusCode.TooManyRequests, throttledResponse.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenPasswordIsIncorrect()
    {
        var email = $"jane-{Guid.NewGuid()}@example.com";
        await RegisterThenLoginAsync(email);

        var response = await _client.PostAsJsonAsync(
            "/api/auth/login", new LoginRequestDto(email, "WrongPassword!"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsProfile_WhenAuthenticatedWithAccessToken()
    {
        var email = $"jane-{Guid.NewGuid()}@example.com";
        var tokens = await RegisterThenLoginAsync(email);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        var response = await _client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<UserResponseDto>(JsonOptions);
        Assert.NotNull(profile);
        Assert.Equal(email, profile!.Email);
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsUnauthorized_WithoutAccessToken()
    {
        var response = await _client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_RotatesToken_AndOldRefreshTokenBecomesInvalid()
    {
        var email = $"jane-{Guid.NewGuid()}@example.com";
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto("Jane Doe", email, "P@ssw0rd!"));
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto(email, "P@ssw0rd!"));
        var originalRefreshToken = GetRefreshTokenCookieValue(loginResponse);

        // Use a client without cookie auto-handling so each request only carries the explicit cookie under test.
        using var manualClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });

        var refreshResponse = await manualClient.SendAsync(CreateRequestWithRefreshCookie("/api/auth/refresh", originalRefreshToken));
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        var rotatedRefreshToken = GetRefreshTokenCookieValue(refreshResponse);
        Assert.NotEqual(originalRefreshToken, rotatedRefreshToken);

        var reuseResponse = await manualClient.SendAsync(CreateRequestWithRefreshCookie("/api/auth/refresh", originalRefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, reuseResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_RevokesToken_SoSubsequentRefreshFails()
    {
        var email = $"jane-{Guid.NewGuid()}@example.com";
        await RegisterThenLoginAsync(email);

        var logoutResponse = await _client.PostAsync("/api/auth/logout", null);
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        var refreshResponse = await _client.PostAsync("/api/auth/refresh", null);
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_IsIdempotent_WhenCalledTwice()
    {
        var email = $"jane-{Guid.NewGuid()}@example.com";
        await RegisterThenLoginAsync(email);

        var firstLogout = await _client.PostAsync("/api/auth/logout", null);
        Assert.Equal(HttpStatusCode.NoContent, firstLogout.StatusCode);

        var secondLogout = await _client.PostAsync("/api/auth/logout", null);
        Assert.Equal(HttpStatusCode.NoContent, secondLogout.StatusCode);
    }
}
