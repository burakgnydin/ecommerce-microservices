using PaymentService.Application.DTOs;

namespace PaymentService.Application.Interfaces;

public interface INotificationClient
{
    /// <summary>
    /// Best-effort notification of a payment outcome for the caller's own order. Implementations
    /// must not throw: notification-service being unavailable should never fail a payment.
    /// </summary>
    Task NotifyAsync(Guid orderId, PaymentNotificationType type, string bearerToken, CancellationToken cancellationToken = default);
}
