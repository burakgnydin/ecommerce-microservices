using ProductService.Application.Common;
using ProductService.Application.DTOs;

namespace ProductService.Application.Interfaces;

public interface IProductService
{
    Task<ProductResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductResponseDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<ProductResponseDto> CreateAsync(ProductCreateDto dto, CancellationToken cancellationToken = default);
    Task<ProductResponseDto> UpdateAsync(Guid id, ProductUpdateDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CheckAvailabilityAsync(Guid productId, int requestedQuantity, CancellationToken cancellationToken = default);
}
