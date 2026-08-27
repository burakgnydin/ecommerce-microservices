namespace PaymentService.Application.DTOs;

/// <summary>
/// Represents the outcome of a payment attempt as returned by the API.
/// </summary>
/// <param name="Id">Unique payment identifier.</param>
/// <param name="OrderId">Id of the order that was paid.</param>
/// <param name="Status">Payment status: Succeeded or Failed.</param>
/// <param name="Amount">Amount charged, resolved from the order.</param>
/// <param name="MaskedCardNumber">Masked card number, e.g. "**** **** **** 1234".</param>
/// <param name="CreatedAt">Date and time the payment was processed, in UTC.</param>
public record PaymentResponseDto(
    Guid Id,
    Guid OrderId,
    string Status,
    decimal Amount,
    string MaskedCardNumber,
    DateTime CreatedAt);
