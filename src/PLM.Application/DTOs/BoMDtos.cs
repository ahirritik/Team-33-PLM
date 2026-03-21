using PLM.Domain.Enums;

namespace PLM.Application.DTOs;

public class BoMDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Status Status { get; set; }
    public int CurrentVersionNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public BoMVersionDto? ActiveVersion { get; set; }
    public int VersionCount { get; set; }
}

public class BoMVersionDto
{
    public int Id { get; set; }
    public int BoMId { get; set; }
    public int VersionNumber { get; set; }
    public bool IsActive { get; set; }
    public string ChangeDescription { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public List<BoMComponentDto> Components { get; set; } = new();
    public List<BoMOperationDto> Operations { get; set; } = new();
    public decimal TotalCost => Components.Sum(c => c.Quantity * c.UnitCost);
}

public class BoMComponentDto
{
    public int Id { get; set; }
    public int BoMVersionId { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string Unit { get; set; } = "pcs";
}

public class BoMOperationDto
{
    public int Id { get; set; }
    public int BoMVersionId { get; set; }
    public string OperationName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SequenceOrder { get; set; }
    public decimal EstimatedTime { get; set; }
    public string WorkCenter { get; set; } = string.Empty;
}

public class BoMCreateDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<BoMComponentDto> Components { get; set; } = new();
    public List<BoMOperationDto> Operations { get; set; } = new();
}
