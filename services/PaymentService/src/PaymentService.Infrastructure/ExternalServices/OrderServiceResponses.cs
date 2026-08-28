namespace PaymentService.Infrastructure.ExternalServices;

/// <summary>
/// Mirrors the subset of order-service's OrderResponseDto that payment-service needs.
/// </summary>
internal record OrderServiceOrderResponse(
    Guid Id,
    Guid UserId,
    string Status,
    decimal TotalAmount);
