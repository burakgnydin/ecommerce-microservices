using PaymentService.Application.DTOs;

namespace PaymentService.Application.Interfaces;

public interface IPaymentService
{
    /// <summary>
    /// Charges a simulated card against the given order and, on success, marks the order
    /// as paid in order-service.
    /// </summary>
    /// <param name="userId">Id of the authenticated caller, resolved from the JWT.</param>
    /// <param name="dto">Payment request payload.</param>
    /// <param name="bearerToken">The caller's own JWT, forwarded to order-service for ownership checks.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<PaymentResponseDto> ChargeAsync(Guid userId, PaymentRequestDto dto, string bearerToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the authenticated user's own payment history, most recent first.
    /// </summary>
    /// <param name="userId">Id of the authenticated caller, resolved from the JWT.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<PaymentResponseDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns every payment across all users, most recent first. Restricted to admins.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<PaymentResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
