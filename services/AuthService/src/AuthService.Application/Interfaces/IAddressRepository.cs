using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IAddressRepository
{
    Task<IReadOnlyList<Address>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Address?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateAsync(Address address, CancellationToken cancellationToken = default);
    Task UpdateAsync(Address address, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
