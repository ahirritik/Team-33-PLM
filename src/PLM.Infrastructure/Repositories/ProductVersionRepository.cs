using Microsoft.EntityFrameworkCore;
using PLM.Domain.Entities;
using PLM.Domain.Interfaces;
using PLM.Infrastructure.Data;

namespace PLM.Infrastructure.Repositories;

public class ProductVersionRepository : IProductVersionRepository
{
    private readonly IDbContextFactory<PlmDbContext> _contextFactory;

    public ProductVersionRepository(IDbContextFactory<PlmDbContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<ProductVersion?> GetByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ProductVersions.FindAsync(id);
    }

    public async Task<IReadOnlyList<ProductVersion>> GetByProductIdAsync(int productId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ProductVersions
            .Where(v => v.ProductId == productId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync();
    }

    public async Task<ProductVersion?> GetActiveVersionAsync(int productId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ProductVersions
            .FirstOrDefaultAsync(v => v.ProductId == productId && v.IsActive);
    }

    public async Task<ProductVersion> AddAsync(ProductVersion version)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.ProductVersions.Add(version);
        await context.SaveChangesAsync();
        return version;
    }

    public async Task UpdateAsync(ProductVersion version)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.ProductVersions.Update(version);
        await context.SaveChangesAsync();
    }
}
