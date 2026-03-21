using PLM.Domain.Enums;

namespace PLM.Domain.Entities;

public class ProductVersion
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int VersionNumber { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public string ChangeDescription { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public string Attachments { get; set; } = string.Empty;

    // Navigation properties
    public Product Product { get; set; } = null!;
}
