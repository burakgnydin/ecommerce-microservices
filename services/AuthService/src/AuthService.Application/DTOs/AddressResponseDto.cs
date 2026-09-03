namespace AuthService.Application.DTOs;

/// <summary>
/// Represents a saved address as returned by the API.
/// </summary>
/// <param name="Id">Unique address identifier.</param>
/// <param name="Title">User-facing label, e.g. "Ev" or "İş".</param>
/// <param name="City">City (il).</param>
/// <param name="District">District (ilçe).</param>
/// <param name="FullAddress">Full street address.</param>
public record AddressResponseDto(
    Guid Id,
    string Title,
    string City,
    string District,
    string FullAddress);
