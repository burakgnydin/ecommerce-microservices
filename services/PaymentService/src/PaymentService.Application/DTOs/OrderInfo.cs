namespace PaymentService.Application.DTOs;

/// <summary>
/// Mirrors the subset of order-service's OrderResponseDto that payment-service needs.
/// </summary>
public record OrderInfo(Guid Id, Guid UserId, string Status, decimal TotalAmount);
