using ProductService.Application.Common;
using ProductService.Application.DTOs;
using ProductService.Application.Exceptions;
using ProductService.Application.Interfaces;
using ProductService.Application.Mapping;

namespace ProductService.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<ProductResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Product '{id}' was not found.");

        return product.ToDto();
    }

    public async Task<PagedResult<ProductResponseDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var paged = await _productRepository.GetAllAsync(pageNumber, pageSize, cancellationToken);
        var items = paged.Items.Select(p => p.ToDto()).ToList();

        return new PagedResult<ProductResponseDto>(items, paged.PageNumber, paged.PageSize, paged.TotalCount);
    }

    public async Task<ProductResponseDto> CreateAsync(ProductCreateDto dto, CancellationToken cancellationToken = default)
    {
        if (!await _categoryRepository.ExistsAsync(dto.CategoryId, cancellationToken))
            throw new InvalidCategoryReferenceException(dto.CategoryId);

        var product = dto.ToEntity();
        await _productRepository.CreateAsync(product, cancellationToken);

        return product.ToDto();
    }

    public async Task<ProductResponseDto> UpdateAsync(Guid id, ProductUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Product '{id}' was not found.");

        if (!await _categoryRepository.ExistsAsync(dto.CategoryId, cancellationToken))
            throw new InvalidCategoryReferenceException(dto.CategoryId);

        product.UpdateDetails(dto.Name, dto.Description, dto.Price, dto.Stock, dto.CategoryId);
        await _productRepository.UpdateAsync(product, cancellationToken);

        return product.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!await _productRepository.ExistsAsync(id, cancellationToken))
            throw new NotFoundException($"Product '{id}' was not found.");

        await _productRepository.DeleteAsync(id, cancellationToken);
    }
}
