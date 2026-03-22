using Microsoft.EntityFrameworkCore;
using PLM.Domain.Entities;
using PLM.Domain.Interfaces;
using PLM.Infrastructure.Data;

namespace PLM.Infrastructure.Repositories;

public class BoMVersionRepository : IBoMVersionRepository
{
    private readonly IDbContextFactory<PlmDbContext> _contextFactory;

    public BoMVersionRepository(IDbContextFactory<PlmDbContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<BoMVersion?> GetByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.BoMVersions.FindAsync(id);
    }

    public async Task<BoMVersion?> GetByIdWithComponentsAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.BoMVersions
            .Include(v => v.Components)
            .Include(v => v.Operations.OrderBy(o => o.SequenceOrder))
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<IReadOnlyList<BoMVersion>> GetByBoMIdAsync(int bomId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.BoMVersions
            .Include(v => v.Components)
            .Include(v => v.Operations.OrderBy(o => o.SequenceOrder))
            .Where(v => v.BoMId == bomId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync();
    }

    public async Task<BoMVersion?> GetActiveVersionAsync(int bomId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.BoMVersions
            .Include(v => v.Components)
            .Include(v => v.Operations.OrderBy(o => o.SequenceOrder))
            .FirstOrDefaultAsync(v => v.BoMId == bomId && v.IsActive);
    }

    public async Task<BoMVersion> AddAsync(BoMVersion version)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.BoMVersions.Add(version);
        await context.SaveChangesAsync();
        return version;
    }

    public async Task UpdateAsync(BoMVersion version)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.BoMVersions.Update(version);
        await context.SaveChangesAsync();
    }
}
