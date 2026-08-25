using ProductService.Application.DTOs;

namespace ProductService.Application.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CategoryResponseDto> CreateAsync(CategoryCreateDto dto, CancellationToken cancellationToken = default);
}
