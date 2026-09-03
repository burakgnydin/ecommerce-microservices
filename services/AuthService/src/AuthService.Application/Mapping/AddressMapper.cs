using AuthService.Application.DTOs;
using AuthService.Domain.Entities;

namespace AuthService.Application.Mapping;

public static class AddressMapper
{
    public static AddressResponseDto ToDto(this Address address)
    {
        return new AddressResponseDto(address.Id, address.Title, address.City, address.District, address.FullAddress);
    }
}
