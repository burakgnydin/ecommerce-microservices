using PaymentService.Domain.Entities;

namespace PaymentService.Application.Interfaces;

public interface IPaymentRepository
{
    Task CreateAsync(Payment payment, CancellationToken cancellationToken = default);
}
