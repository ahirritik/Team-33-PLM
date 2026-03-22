using Microsoft.EntityFrameworkCore;
using PLM.Domain.Entities;
using PLM.Domain.Enums;
using PLM.Domain.Interfaces;
using PLM.Infrastructure.Data;

namespace PLM.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IDbContextFactory<PlmDbContext> _contextFactory;

    public ProductRepository(IDbContextFactory<PlmDbContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<Product?> GetByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Products.FindAsync(id);
    }

    public async Task<Product?> GetByIdWithVersionsAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Products
            .Include(p => p.Versions.OrderByDescending(v => v.VersionNumber))
            .Include(p => p.BoMs)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(Status? status = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Products
            .Include(p => p.Versions.Where(v => v.IsActive))
            .Include(p => p.BoMs)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        return await query.OrderByDescending(p => p.UpdatedAt).ToListAsync();
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search = null, Status? status = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Products
            .Include(p => p.Versions.Where(v => v.IsActive))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Product> AddAsync(Product product)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Products.Add(product);
        await context.SaveChangesAsync();
        return product;
    }

    public async Task UpdateAsync(Product product)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Products.Update(product);
        await context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Products.AnyAsync(p => p.Id == id);
    }
}
