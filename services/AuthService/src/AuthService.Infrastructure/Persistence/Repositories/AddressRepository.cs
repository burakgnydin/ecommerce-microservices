using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Persistence.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly AuthDbContext _context;

    public AddressRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Address>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Addresses
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Address?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Addresses.SingleOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task CreateAsync(Address address, CancellationToken cancellationToken = default)
    {
        await _context.Addresses.AddAsync(address, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Address address, CancellationToken cancellationToken = default)
    {
        _context.Addresses.Update(address);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var address = await _context.Addresses.SingleOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (address is null)
            return;

        _context.Addresses.Remove(address);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
