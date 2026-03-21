using PLM.Domain.Entities;

namespace PLM.Domain.Interfaces;

public interface IAuditLogRepository
{
    Task<AuditLog> AddAsync(AuditLog auditLog);
    Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityType, int entityId);
    Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? entityType = null, string? userId = null);
}
