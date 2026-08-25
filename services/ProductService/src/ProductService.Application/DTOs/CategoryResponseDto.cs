namespace ProductService.Application.DTOs;

/// <summary>
/// Represents a category returned by the API.
/// </summary>
/// <param name="Id">Unique category identifier.</param>
/// <param name="Name">Category name.</param>
public record CategoryResponseDto(
    Guid Id,
    string Name);
