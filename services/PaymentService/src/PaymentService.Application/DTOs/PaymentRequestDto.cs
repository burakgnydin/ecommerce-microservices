namespace PaymentService.Application.DTOs;

/// <summary>
/// Payload used to charge a card against an order. The amount is never taken from the
/// caller; it is always resolved from the order itself via order-service.
/// </summary>
/// <param name="OrderId">Id of the order being paid.</param>
/// <param name="CardNumber">Card number (13-19 digits). Never stored or logged in full.</param>
/// <param name="ExpiryMonth">Card expiry month (1-12).</param>
/// <param name="ExpiryYear">Card expiry year.</param>
/// <param name="Cvv">Card security code (3-4 digits).</param>
public record PaymentRequestDto(
    Guid OrderId,
    string CardNumber,
    int ExpiryMonth,
    int ExpiryYear,
    string Cvv);
