using ProductService.Application.DTOs;
using ProductService.Application.Exceptions;
using ProductService.Application.Interfaces;
using ProductService.Application.Mapping;

namespace ProductService.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;

    public CategoryService(ICategoryRepository categoryRepository, IProductRepository productRepository)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
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

    public async Task<CategoryResponseDto> UpdateAsync(Guid id, CategoryUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Category '{id}' was not found.");

        if (dto.Name != category.Name && await _categoryRepository.ExistsByNameAsync(dto.Name, cancellationToken))
            throw new DuplicateCategoryException(dto.Name);

        category.UpdateName(dto.Name);
        await _categoryRepository.UpdateAsync(category, cancellationToken);

        return category.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!await _categoryRepository.ExistsAsync(id, cancellationToken))
            throw new NotFoundException($"Category '{id}' was not found.");

        if (await _productRepository.ExistsByCategoryIdAsync(id, cancellationToken))
            throw new CategoryInUseException(id);

        await _categoryRepository.DeleteAsync(id, cancellationToken);
    }
}
