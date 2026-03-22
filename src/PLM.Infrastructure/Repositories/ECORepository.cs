using Microsoft.EntityFrameworkCore;
using PLM.Domain.Entities;
using PLM.Domain.Enums;
using PLM.Domain.Interfaces;
using PLM.Infrastructure.Data;

namespace PLM.Infrastructure.Repositories;

public class ECORepository : IECORepository
{
    private readonly IDbContextFactory<PlmDbContext> _contextFactory;

    public ECORepository(IDbContextFactory<PlmDbContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<ECO?> GetByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ECOs
            .Include(e => e.Product)
            .Include(e => e.BoM)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<ECO?> GetByIdWithApprovalsAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ECOs
            .Include(e => e.Product)
            .Include(e => e.BoM)
            .Include(e => e.Approvals.OrderByDescending(a => a.CreatedAt))
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IReadOnlyList<ECO>> GetAllAsync(ECOStage? stage = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.ECOs
            .Include(e => e.Product)
            .Include(e => e.BoM)
            .Include(e => e.Approvals)
            .AsQueryable();

        if (stage.HasValue)
            query = query.Where(e => e.Stage == stage.Value);

        return await query.OrderByDescending(e => e.CreatedAt).ToListAsync();
    }

    public async Task<(IReadOnlyList<ECO> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, ECOStage? stage = null, ECOType? type = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.ECOs
            .Include(e => e.Product)
            .Include(e => e.BoM)
            .Include(e => e.Approvals)
            .AsQueryable();

        if (stage.HasValue)
            query = query.Where(e => e.Stage == stage.Value);
        if (type.HasValue)
            query = query.Where(e => e.Type == type.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<ECO>> GetByProductIdAsync(int productId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ECOs
            .Include(e => e.Approvals)
            .Where(e => e.ProductId == productId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ECO>> GetPendingApprovalsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ECOs
            .Include(e => e.Product)
            .Include(e => e.BoM)
            .Where(e => e.Stage == ECOStage.Approval)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<ECO> AddAsync(ECO eco)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.ECOs.Add(eco);
        await context.SaveChangesAsync();
        return eco;
    }

    public async Task UpdateAsync(ECO eco)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.ECOs.Update(eco);
        await context.SaveChangesAsync();
    }

    public async Task<int> GetCountByStageAsync(ECOStage stage)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ECOs.CountAsync(e => e.Stage == stage);
    }
}
