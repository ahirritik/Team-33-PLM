using Microsoft.EntityFrameworkCore;
using PLM.Domain.Entities;
using PLM.Domain.Enums;
using PLM.Domain.Interfaces;
using PLM.Infrastructure.Data;

namespace PLM.Infrastructure.Repositories;

public class BoMRepository : IBoMRepository
{
    private readonly PlmDbContext _context;

    public BoMRepository(PlmDbContext context) => _context = context;

    public async Task<BoM?> GetByIdAsync(int id)
        => await _context.BoMs.FindAsync(id);

    public async Task<BoM?> GetByIdWithVersionsAsync(int id)
        => await _context.BoMs
            .Include(b => b.Product)
            .Include(b => b.Versions.OrderByDescending(v => v.VersionNumber))
                .ThenInclude(v => v.Components)
            .Include(b => b.Versions)
                .ThenInclude(v => v.Operations.OrderBy(o => o.SequenceOrder))
            .FirstOrDefaultAsync(b => b.Id == id);

    public async Task<IReadOnlyList<BoM>> GetByProductIdAsync(int productId)
        => await _context.BoMs
            .Include(b => b.Product)
            .Include(b => b.Versions.Where(v => v.IsActive))
                .ThenInclude(v => v.Components)
            .Where(b => b.ProductId == productId)
            .OrderByDescending(b => b.UpdatedAt)
            .ToListAsync();

    public async Task<(IReadOnlyList<BoM> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search = null, Status? status = null)
    {
        var query = _context.BoMs
            .Include(b => b.Product)
            .Include(b => b.Versions.Where(v => v.IsActive))
                .ThenInclude(v => v.Components)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(b => b.Name.Contains(search) || b.Description.Contains(search));

        if (status.HasValue)
            query = query.Where(b => b.Status == status.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<BoM> AddAsync(BoM bom)
    {
        _context.BoMs.Add(bom);
        await _context.SaveChangesAsync();
        return bom;
    }

    public async Task UpdateAsync(BoM bom)
    {
        _context.BoMs.Update(bom);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
        => await _context.BoMs.AnyAsync(b => b.Id == id);
}
