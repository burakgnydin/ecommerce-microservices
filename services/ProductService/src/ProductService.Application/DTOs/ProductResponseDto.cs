namespace ProductService.Application.DTOs;

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
