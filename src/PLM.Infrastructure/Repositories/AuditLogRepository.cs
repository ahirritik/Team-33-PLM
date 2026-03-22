using Microsoft.EntityFrameworkCore;
using PLM.Domain.Entities;
using PLM.Domain.Interfaces;
using PLM.Infrastructure.Data;

namespace PLM.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly IDbContextFactory<PlmDbContext> _contextFactory;

    public AuditLogRepository(IDbContextFactory<PlmDbContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<AuditLog> AddAsync(AuditLog auditLog)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.AuditLogs.Add(auditLog);
        await context.SaveChangesAsync();
        return auditLog;
    }

    public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityType, int entityId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.AuditLogs
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? entityType = null, string? userId = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.EntityType == entityType);
        if (!string.IsNullOrWhiteSpace(userId))
            query = query.Where(a => a.UserId == userId);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
