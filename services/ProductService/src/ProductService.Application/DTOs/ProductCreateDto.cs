namespace ProductService.Application.DTOs;

/// <summary>
/// Payload used to create a new product.
/// </summary>
/// <param name="Name">Product name (max 200 characters).</param>
/// <param name="Description">Optional product description (max 2000 characters).</param>
/// <param name="Price">Product price. Cannot be negative.</param>
/// <param name="Stock">Available stock quantity. Cannot be negative.</param>
/// <param name="CategoryId">Id of an existing category the product belongs to.</param>
/// <param name="AllowsPreOrder">Whether the product can be ordered beyond available stock.</param>
/// <param name="ImageUrl">Optional URL of a product image.</param>
public record ProductCreateDto(
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    Guid CategoryId,
    bool AllowsPreOrder = false,
    string? ImageUrl = null);
