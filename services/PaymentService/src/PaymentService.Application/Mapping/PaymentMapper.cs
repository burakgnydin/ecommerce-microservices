using PaymentService.Application.DTOs;
using PaymentService.Domain.Entities;

namespace PaymentService.Application.Mapping;

public static class PaymentMapper
{
    public static PaymentResponseDto ToDto(this Payment payment)
    {
        return new PaymentResponseDto(
            payment.Id,
            payment.OrderId,
            payment.Status.ToString(),
            payment.Amount,
            payment.MaskedCardNumber,
            payment.CreatedAt);
    }
}
