using Microsoft.EntityFrameworkCore;
using ProductService.Application.Common;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Product>> GetAllAsync(int pageNumber, int pageSize, Guid? categoryId = null, string? search = null, CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = _context.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .Where(p => categoryId == null || p.CategoryId == categoryId);

        if (string.IsNullOrWhiteSpace(search))
        {
            query = query.OrderBy(p => p.CreatedAt);
        }
        else
        {
            query = query
                .Where(p => EF.Functions.ILike(p.Name, $"%{search}%")
                    || (p.Description != null && EF.Functions.ILike(p.Description, $"%{search}%"))
                    || EF.Functions.TrigramsWordSimilarity(search, p.Name) > 0.4)
                .OrderByDescending(p => EF.Functions.TrigramsWordSimilarity(search, p.Name));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Product>(items, pageNumber, pageSize, totalCount);
    }

    public async Task CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product is null)
            return;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products.AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Products.AnyAsync(p => p.CategoryId == categoryId, cancellationToken);
    }
}
