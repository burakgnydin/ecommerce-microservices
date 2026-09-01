using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using AuthService.Application.DTOs;
using AuthService.IntegrationTests.Fixtures;

namespace AuthService.IntegrationTests.Api;

[Collection("Integration")]
public class SeedAdminApiTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private AuthServiceApiFactory _factory = null!;
    private HttpClient _client = null!;
    private readonly string _seedEmail = $"seed-admin-{Guid.NewGuid()}@example.com";
    private const string SeedPassword = "P@ssw0rd!";

    public SeedAdminApiTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync()
    {
        _factory = new AuthServiceApiFactory(_fixture.ConnectionString, _seedEmail, SeedPassword);
        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Login_Succeeds_WithAdminRole_ForSeededAdmin()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login", new LoginRequestDto(_seedEmail, SeedPassword));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var tokens = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(tokens);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(tokens!.AccessToken);
        var role = jwt.Claims.Single(c => c.Type == ClaimTypes.Role).Value;
        Assert.Equal("Admin", role);
    }

    [Fact]
    public async Task Register_StillCreatesCustomer_WhenSeedAdminIsConfigured()
    {
        var email = $"jane-{Guid.NewGuid()}@example.com";
        var registerDto = new RegisterRequestDto("Jane Doe", email, "P@ssw0rd!");

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login", new LoginRequestDto(email, "P@ssw0rd!"));
        var tokens = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(tokens!.AccessToken);
        var role = jwt.Claims.Single(c => c.Type == ClaimTypes.Role).Value;
        Assert.Equal("Customer", role);
    }
}
