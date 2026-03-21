using PLM.Application.DTOs;

namespace PLM.Application.Interfaces;

public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto?> GetByIdWithVersionsAsync(int id);
    Task<(List<ProductDto> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search = null);
    Task<ProductDto> CreateAsync(ProductCreateDto dto, string userId, string userName);
    Task<List<ProductVersionDto>> GetVersionHistoryAsync(int productId);
}

public interface IBoMService
{
    Task<BoMDto?> GetByIdAsync(int id);
    Task<BoMDto?> GetByIdWithVersionsAsync(int id);
    Task<(List<BoMDto> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search = null);
    Task<List<BoMDto>> GetByProductIdAsync(int productId);
    Task<BoMDto> CreateAsync(BoMCreateDto dto, string userId, string userName);
    Task<BoMVersionDto?> GetVersionWithComponentsAsync(int versionId);
    Task<List<BoMVersionDto>> GetVersionHistoryAsync(int bomId);
}

public interface IECOService
{
    Task<ECODto?> GetByIdAsync(int id);
    Task<(List<ECODto> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? stageFilter = null, string? typeFilter = null);
    Task<ECODto> CreateAsync(ECOCreateDto dto, string userId, string userName);
    Task SubmitAsync(int ecoId, string userId, string userName);
    Task ApproveAsync(ECOApprovalCreateDto dto, string userId, string userName);
    Task RejectAsync(ECOApprovalCreateDto dto, string userId, string userName);
    Task<List<ECODto>> GetPendingApprovalsAsync();
    Task<List<ComparisonDto>> GetComparisonAsync(int ecoId);
    Task<DashboardDto> GetDashboardAsync();
}

public interface IAuditService
{
    Task LogAsync(string action, string entityType, int entityId, string? oldValue, string? newValue, string userId, string userName);
    Task<(List<AuditLogDto> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? entityType = null, string? userId = null);
    Task<List<AuditLogDto>> GetByEntityAsync(string entityType, int entityId);
}
