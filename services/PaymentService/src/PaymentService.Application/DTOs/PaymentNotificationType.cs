namespace PaymentService.Application.DTOs;

/// <summary>
/// Mirrors the subset of notification-service's NotificationType enum that payment-service raises.
/// Values are pinned to match notification-service's numeric JSON serialization.
/// </summary>
public enum PaymentNotificationType
{
    PaymentSucceeded = 1,
    PaymentFailed = 2
}
