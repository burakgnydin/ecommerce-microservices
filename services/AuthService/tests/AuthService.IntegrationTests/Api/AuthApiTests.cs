using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuthService.Application.DTOs;
using AuthService.IntegrationTests.Fixtures;

namespace AuthService.IntegrationTests.Api;

[Collection("Integration")]
public class AuthApiTests : IAsyncLifetime
{
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

    private async Task<LoginResponseDto> RegisterThenLoginAsync(string email, string password = "P@ssw0rd!")
    {
        var registerDto = new RegisterRequestDto("Jane Doe", email, password);
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginDto = new LoginRequestDto(email, password);
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var tokens = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(tokens);
        return tokens!;
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
        var profile = await response.Content.ReadFromJsonAsync<UserResponseDto>();
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
        var tokens = await RegisterThenLoginAsync(email);

        var refreshResponse = await _client.PostAsJsonAsync(
            "/api/auth/refresh", new RefreshRequestDto(tokens.RefreshToken));
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        var rotated = await refreshResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(rotated);
        Assert.NotEqual(tokens.RefreshToken, rotated!.RefreshToken);

        var reuseResponse = await _client.PostAsJsonAsync(
            "/api/auth/refresh", new RefreshRequestDto(tokens.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, reuseResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_RevokesToken_SoSubsequentRefreshFails()
    {
        var email = $"jane-{Guid.NewGuid()}@example.com";
        var tokens = await RegisterThenLoginAsync(email);

        var logoutResponse = await _client.PostAsJsonAsync(
            "/api/auth/logout", new LogoutRequestDto(tokens.RefreshToken));
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        var refreshResponse = await _client.PostAsJsonAsync(
            "/api/auth/refresh", new RefreshRequestDto(tokens.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_IsIdempotent_WhenCalledTwice()
    {
        var email = $"jane-{Guid.NewGuid()}@example.com";
        var tokens = await RegisterThenLoginAsync(email);

        var firstLogout = await _client.PostAsJsonAsync(
            "/api/auth/logout", new LogoutRequestDto(tokens.RefreshToken));
        Assert.Equal(HttpStatusCode.NoContent, firstLogout.StatusCode);

        var secondLogout = await _client.PostAsJsonAsync(
            "/api/auth/logout", new LogoutRequestDto(tokens.RefreshToken));
        Assert.Equal(HttpStatusCode.NoContent, secondLogout.StatusCode);
    }
}
