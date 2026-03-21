using PLM.Domain.Enums;

namespace PLM.Application.DTOs;

public class ECODto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ECOType Type { get; set; }
    public ECOStage Stage { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int? BoMId { get; set; }
    public string? BoMName { get; set; }
    public DateTime EffectiveDate { get; set; }
    public bool CreateNewVersion { get; set; }
    public decimal? ProposedCostPrice { get; set; }
    public decimal? ProposedSalePrice { get; set; }
    public string? ProposedComponents { get; set; }
    public string? ProposedOperations { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ECOApprovalDto> Approvals { get; set; } = new();
}

public class ECOCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ECOType Type { get; set; }
    public int ProductId { get; set; }
    public int? BoMId { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public bool CreateNewVersion { get; set; } = true;
    public decimal? ProposedCostPrice { get; set; }
    public decimal? ProposedSalePrice { get; set; }
    public List<BoMComponentDto>? ProposedComponents { get; set; }
    public List<BoMOperationDto>? ProposedOperations { get; set; }
}

public class ECOApprovalDto
{
    public int Id { get; set; }
    public int ECOId { get; set; }
    public string ApproverId { get; set; } = string.Empty;
    public string ApproverName { get; set; } = string.Empty;
    public ApprovalDecision Decision { get; set; }
    public string Comments { get; set; } = string.Empty;
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ECOApprovalCreateDto
{
    public int ECOId { get; set; }
    public ApprovalDecision Decision { get; set; }
    public string Comments { get; set; } = string.Empty;
}
