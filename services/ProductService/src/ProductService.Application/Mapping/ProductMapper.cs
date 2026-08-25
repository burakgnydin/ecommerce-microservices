using ProductService.Application.DTOs;
using ProductService.Domain.Entities;

namespace ProductService.Application.Mapping;

public static class ProductMapper
{
    public static ProductResponseDto ToDto(this Product product)
    {
        return new ProductResponseDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.CategoryId,
            product.Category?.Name,
            product.AllowsPreOrder,
            product.CreatedAt);
    }

    public static Product ToEntity(this ProductCreateDto dto)
    {
        return new Product(dto.Name, dto.Description, dto.Price, dto.Stock, dto.CategoryId, dto.AllowsPreOrder);
    }
}
