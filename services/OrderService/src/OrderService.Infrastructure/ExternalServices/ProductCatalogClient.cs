using System.Net;
using System.Net.Http.Json;
using OrderService.Application.DTOs;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces;

namespace OrderService.Infrastructure.ExternalServices;

public class ProductCatalogClient : IProductCatalogClient
{
    private readonly HttpClient _httpClient;

    public ProductCatalogClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductCatalogItem?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync($"api/products/{productId}", cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new ProductServiceUnavailableException("product-service could not be reached.", ex);
        }
        catch (TimeoutException ex)
        {
            throw new ProductServiceUnavailableException("product-service did not respond in time.", ex);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new ProductServiceUnavailableException(
                $"product-service returned an unexpected status code: {(int)response.StatusCode}.");
        }

        var product = await response.Content.ReadFromJsonAsync<ProductServiceProductResponse>(cancellationToken)
            ?? throw new ProductServiceUnavailableException("product-service returned an empty response.");

        return new ProductCatalogItem(product.Id, product.Name, product.Price, product.Stock);
    }
}
