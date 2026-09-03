using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Infrastructure.Persistence.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;

    public PaymentRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        await _context.Payments.AddAsync(payment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
