using PLM.Domain.Enums;

namespace PLM.Application.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Status Status { get; set; }
    public int CurrentVersionNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ProductVersionDto? ActiveVersion { get; set; }
    public int VersionCount { get; set; }
}

public class ProductVersionDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int VersionNumber { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public string ChangeDescription { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Attachments { get; set; }
}

public class ProductCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public string? Attachments { get; set; }
}
