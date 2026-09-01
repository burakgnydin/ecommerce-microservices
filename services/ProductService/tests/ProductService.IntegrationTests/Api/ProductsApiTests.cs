using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ProductService.Application.DTOs;
using ProductService.Domain.Entities;
using ProductService.IntegrationTests.Fixtures;

namespace ProductService.IntegrationTests.Api;

[Collection("Integration")]
public class ProductsApiTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private ProductServiceApiFactory _factory = null!;
    private HttpClient _client = null!;

    public ProductsApiTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync()
    {
        _factory = new ProductServiceApiFactory(_fixture.ConnectionString);
        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    private void AuthenticateAsAdmin()
        => _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateAccessToken("Admin"));

    [Fact]
    public async Task CreateThenGet_ReturnsCreatedProduct_WhenCallerIsAdmin()
    {
        await using var context = _fixture.CreateDbContext();
        var category = new Category($"Category-{Guid.NewGuid()}");
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        AuthenticateAsAdmin();
        var createDto = new ProductCreateDto("Widget", "A useful widget", 9.99m, 10, category.Id);

        var createResponse = await _client.PostAsJsonAsync("/api/products", createDto);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponseDto>();
        Assert.NotNull(created);
        Assert.Equal("Widget", created!.Name);

        var getResponse = await _client.GetAsync($"/api/products/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<ProductResponseDto>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal("Widget", fetched.Name);
        Assert.Equal(category.Id, fetched.CategoryId);
    }

    [Fact]
    public async Task Create_ReturnsUnauthorized_WhenNoTokenIsProvided()
    {
        var createDto = new ProductCreateDto("Widget", "A useful widget", 9.99m, 10, Guid.NewGuid());

        var response = await _client.PostAsJsonAsync("/api/products", createDto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsForbidden_WhenCallerIsNotAdmin()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateAccessToken("Customer"));
        var createDto = new ProductCreateDto("Widget", "A useful widget", 9.99m, 10, Guid.NewGuid());

        var response = await _client.PostAsJsonAsync("/api/products", createDto);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_Succeeds_WithoutAuthentication()
    {
        var response = await _client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
