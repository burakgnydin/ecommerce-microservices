using ProductService.Application.DTOs;
using ProductService.Application.Exceptions;
using ProductService.Application.Interfaces;
using ProductService.Application.Mapping;

namespace ProductService.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IReadOnlyList<CategoryResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var paged = await _categoryRepository.GetAllAsync(pageNumber: 1, pageSize: int.MaxValue, cancellationToken);

        return paged.Items.Select(c => c.ToDto()).ToList();
    }

    public async Task<CategoryResponseDto> CreateAsync(CategoryCreateDto dto, CancellationToken cancellationToken = default)
    {
        if (await _categoryRepository.ExistsByNameAsync(dto.Name, cancellationToken))
            throw new DuplicateCategoryException(dto.Name);

        var category = dto.ToEntity();
        await _categoryRepository.CreateAsync(category, cancellationToken);

        return category.ToDto();
    }
}
