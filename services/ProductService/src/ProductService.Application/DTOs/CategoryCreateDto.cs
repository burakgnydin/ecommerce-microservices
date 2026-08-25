namespace ProductService.Application.DTOs;

/// <summary>
/// Payload used to create a new category.
/// </summary>
/// <param name="Name">Category name (max 100 characters).</param>
public record CategoryCreateDto(string Name);
