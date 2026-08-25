using ProductService.Application.Common;
using ProductService.Domain.Entities;

namespace ProductService.Application.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<Category>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateAsync(Category category, CancellationToken cancellationToken = default);
}
