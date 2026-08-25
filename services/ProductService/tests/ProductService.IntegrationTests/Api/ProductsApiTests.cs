using System.Net;
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

    [Fact]
    public async Task CreateThenGet_ReturnsCreatedProduct()
    {
        await using var context = _fixture.CreateDbContext();
        var category = new Category($"Category-{Guid.NewGuid()}");
        context.Categories.Add(category);
        await context.SaveChangesAsync();

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
}
