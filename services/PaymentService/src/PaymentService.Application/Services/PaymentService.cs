using PaymentService.Application.DTOs;
using PaymentService.Application.Exceptions;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Mapping;
using PaymentService.Domain.Entities;

namespace PaymentService.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderClient _orderClient;

    public PaymentService(IPaymentRepository paymentRepository, IOrderClient orderClient)
    {
        _paymentRepository = paymentRepository;
        _orderClient = orderClient;
    }

    public async Task<PaymentResponseDto> ChargeAsync(Guid userId, PaymentRequestDto dto, string bearerToken, CancellationToken cancellationToken = default)
    {
        var order = await _orderClient.GetOrderAsync(dto.OrderId, bearerToken, cancellationToken)
            ?? throw new NotFoundException($"Order '{dto.OrderId}' was not found.");

        if (order.Status != "Pending")
        {
            throw new OrderNotPayableException($"Order '{dto.OrderId}' cannot be paid while in '{order.Status}' status.");
        }

        var maskedCardNumber = CardPaymentSimulator.Mask(dto.CardNumber);
        var approved = CardPaymentSimulator.IsApproved(dto.CardNumber);

        var payment = approved
            ? Payment.Succeeded(order.Id, userId, order.TotalAmount, maskedCardNumber)
            : Payment.Failed(order.Id, userId, order.TotalAmount, maskedCardNumber);

        await _paymentRepository.CreateAsync(payment, cancellationToken);

        if (!approved)
        {
            throw new PaymentDeclinedException($"Payment for order '{dto.OrderId}' was declined.");
        }

        await _orderClient.MarkAsPaidAsync(order.Id, bearerToken, cancellationToken);

        return payment.ToDto();
    }
}
