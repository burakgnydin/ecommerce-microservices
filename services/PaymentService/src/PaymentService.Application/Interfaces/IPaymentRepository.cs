using PaymentService.Domain.Entities;

namespace PaymentService.Application.Interfaces;

public interface IPaymentRepository
{
    Task CreateAsync(Payment payment, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetAllAsync(CancellationToken cancellationToken = default);
}
