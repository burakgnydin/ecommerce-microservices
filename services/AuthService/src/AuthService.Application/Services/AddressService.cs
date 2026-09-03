using AuthService.Application.DTOs;
using AuthService.Application.Exceptions;
using AuthService.Application.Interfaces;
using AuthService.Application.Mapping;
using AuthService.Domain.Entities;

namespace AuthService.Application.Services;

public class AddressService : IAddressService
{
    private readonly IAddressRepository _addressRepository;

    public AddressService(IAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task<IReadOnlyList<AddressResponseDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var addresses = await _addressRepository.GetByUserIdAsync(userId, cancellationToken);
        return addresses.Select(a => a.ToDto()).ToList();
    }

    public async Task<AddressResponseDto> CreateAsync(Guid userId, AddressCreateDto dto, CancellationToken cancellationToken = default)
    {
        var address = new Address(userId, dto.Title, dto.City, dto.District, dto.FullAddress);
        await _addressRepository.CreateAsync(address, cancellationToken);
        return address.ToDto();
    }

    public async Task<AddressResponseDto> UpdateAsync(Guid userId, Guid addressId, AddressUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var address = await GetOwnedAddressAsync(userId, addressId, cancellationToken);

        address.Update(dto.Title, dto.City, dto.District, dto.FullAddress);
        await _addressRepository.UpdateAsync(address, cancellationToken);

        return address.ToDto();
    }

    public async Task DeleteAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default)
    {
        await GetOwnedAddressAsync(userId, addressId, cancellationToken);
        await _addressRepository.DeleteAsync(addressId, cancellationToken);
    }

    // Not-found and not-owned both resolve to the same NotFoundException so a caller cannot
    // distinguish "doesn't exist" from "belongs to someone else" (avoids IDOR enumeration).
    private async Task<Address> GetOwnedAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken)
    {
        var address = await _addressRepository.GetByIdAsync(addressId, cancellationToken);
        if (address is null || address.UserId != userId)
        {
            throw new NotFoundException($"Address '{addressId}' was not found.");
        }

        return address;
    }
}
