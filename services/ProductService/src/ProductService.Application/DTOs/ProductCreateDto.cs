namespace ProductService.Application.DTOs;

public record ProductCreateDto(
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    Guid CategoryId,
    bool AllowsPreOrder = false);
