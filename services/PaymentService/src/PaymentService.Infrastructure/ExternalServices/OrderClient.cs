using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PaymentService.Application.DTOs;
using PaymentService.Application.Exceptions;
using PaymentService.Application.Interfaces;

namespace PaymentService.Infrastructure.ExternalServices;

public class OrderClient : IOrderClient
{
    private readonly HttpClient _httpClient;

    public OrderClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<OrderInfo?> GetOrderAsync(Guid orderId, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/orders/{orderId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new OrderServiceUnavailableException("order-service could not be reached.", ex);
        }
        catch (TimeoutException ex)
        {
            throw new OrderServiceUnavailableException("order-service did not respond in time.", ex);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new OrderServiceUnavailableException(
                $"order-service returned an unexpected status code: {(int)response.StatusCode}.");
        }

        var order = await response.Content.ReadFromJsonAsync<OrderServiceOrderResponse>(cancellationToken)
            ?? throw new OrderServiceUnavailableException("order-service returned an empty response.");

        return new OrderInfo(order.Id, order.UserId, order.Status, order.TotalAmount);
    }

    public async Task MarkAsPaidAsync(Guid orderId, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, $"api/orders/{orderId}")
        {
            Content = JsonContent.Create(new { Status = "Paid" })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new OrderServiceUnavailableException("order-service could not be reached.", ex);
        }
        catch (TimeoutException ex)
        {
            throw new OrderServiceUnavailableException("order-service did not respond in time.", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new OrderServiceUnavailableException(
                $"order-service returned an unexpected status code while marking order as paid: {(int)response.StatusCode}.");
        }
    }
}
