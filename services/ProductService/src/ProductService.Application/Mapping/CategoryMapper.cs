using ProductService.Application.DTOs;
using ProductService.Domain.Entities;

namespace ProductService.Application.Mapping;

public static class CategoryMapper
{
    public static CategoryResponseDto ToDto(this Category category)
    {
        return new CategoryResponseDto(category.Id, category.Name);
    }

    public static Category ToEntity(this CategoryCreateDto dto)
    {
        return new Category(dto.Name);
    }
}
