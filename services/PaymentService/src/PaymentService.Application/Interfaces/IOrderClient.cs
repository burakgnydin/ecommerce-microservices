using PaymentService.Application.DTOs;

namespace PaymentService.Application.Interfaces;

public interface IOrderClient
{
    /// <summary>
    /// Returns the order, or null if order-service reports it does not exist or is not
    /// owned by the caller (the bearer token's subject). order-service collapses both
    /// cases into the same 404 response to avoid IDOR enumeration.
    /// </summary>
    /// <exception cref="Exceptions.OrderServiceUnavailableException">
    /// Thrown when order-service cannot be reached after retries.
    /// </exception>
    Task<OrderInfo?> GetOrderAsync(Guid orderId, string bearerToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Transitions the order to "Paid" on behalf of the caller identified by the bearer token.
    /// </summary>
    /// <exception cref="Exceptions.OrderServiceUnavailableException">
    /// Thrown when order-service cannot be reached or rejects the transition.
    /// </exception>
    Task MarkAsPaidAsync(Guid orderId, string bearerToken, CancellationToken cancellationToken = default);
}
