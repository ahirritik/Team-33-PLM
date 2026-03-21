using PLM.Domain.Enums;

namespace PLM.Domain.Entities;

public class ECOApproval
{
    public int Id { get; set; }
    public int ECOId { get; set; }
    public string ApproverId { get; set; } = string.Empty;
    public string ApproverName { get; set; } = string.Empty;
    public ApprovalDecision Decision { get; set; } = ApprovalDecision.Pending;
    public string Comments { get; set; } = string.Empty;
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ECO ECO { get; set; } = null!;
}
