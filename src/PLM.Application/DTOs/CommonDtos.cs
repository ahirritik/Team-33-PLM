namespace PLM.Application.DTOs;

public class DashboardDto
{
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int TotalBoMs { get; set; }
    public int PendingECOs { get; set; }
    public int ApprovedECOs { get; set; }
    public int RejectedECOs { get; set; }
    public List<ECODto> RecentECOs { get; set; } = new();
    public List<AuditLogDto> RecentActivity { get; set; } = new();
}

public class AuditLogDto
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class ComparisonDto
{
    public string FieldName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ChangeType { get; set; } = "Unchanged"; // Added, Removed, Modified, Unchanged
}

public class ReportFilterDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? ProductId { get; set; }
    public string? EntityType { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
