using AutoMapper;
using PLM.Application.DTOs;
using PLM.Application.Interfaces;
using PLM.Domain.Entities;
using PLM.Domain.Interfaces;

namespace PLM.Application.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditRepo;
    private readonly IMapper _mapper;

    public AuditService(IAuditLogRepository auditRepo, IMapper mapper)
    {
        _auditRepo = auditRepo;
        _mapper = mapper;
    }

    public async Task LogAsync(string action, string entityType, int entityId,
        string? oldValue, string? newValue, string userId, string userName)
    {
        var log = new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValue = oldValue,
            NewValue = newValue,
            UserId = userId,
            UserName = userName,
            Timestamp = DateTime.UtcNow
        };

        await _auditRepo.AddAsync(log);
    }

    public async Task<(List<AuditLogDto> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? entityType = null, string? userId = null)
    {
        var (items, totalCount) = await _auditRepo.GetPagedAsync(page, pageSize, entityType, userId);
        return (_mapper.Map<List<AuditLogDto>>(items), totalCount);
    }

    public async Task<List<AuditLogDto>> GetByEntityAsync(string entityType, int entityId)
    {
        var logs = await _auditRepo.GetByEntityAsync(entityType, entityId);
        return _mapper.Map<List<AuditLogDto>>(logs);
    }
}
