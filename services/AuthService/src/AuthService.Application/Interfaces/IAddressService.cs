using AuthService.Application.DTOs;

namespace AuthService.Application.Interfaces;

public interface IAddressService
{
    Task<IReadOnlyList<AddressResponseDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AddressResponseDto> CreateAsync(Guid userId, AddressCreateDto dto, CancellationToken cancellationToken = default);
    Task<AddressResponseDto> UpdateAsync(Guid userId, Guid addressId, AddressUpdateDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default);
}
