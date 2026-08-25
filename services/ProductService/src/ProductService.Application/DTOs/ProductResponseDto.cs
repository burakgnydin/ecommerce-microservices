namespace ProductService.Application.DTOs;

/// <summary>
/// Represents a product returned by the API.
/// </summary>
/// <param name="Id">Unique product identifier.</param>
/// <param name="Name">Product name.</param>
/// <param name="Description">Product description, if any.</param>
/// <param name="Price">Product price.</param>
/// <param name="Stock">Available stock quantity.</param>
/// <param name="CategoryId">Id of the category the product belongs to.</param>
/// <param name="CategoryName">Name of the category the product belongs to.</param>
/// <param name="AllowsPreOrder">Whether the product can be ordered beyond available stock.</param>
/// <param name="CreatedAt">Date and time the product was created, in UTC.</param>
public record ProductResponseDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    Guid CategoryId,
    string? CategoryName,
    bool AllowsPreOrder,
    DateTime CreatedAt);
