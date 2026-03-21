using PLM.Domain.Enums;

namespace PLM.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Status Status { get; set; } = Status.Active;
    public int CurrentVersionNumber { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }

    // Navigation properties
    public ICollection<ProductVersion> Versions { get; set; } = new List<ProductVersion>();
    public ICollection<BoM> BoMs { get; set; } = new List<BoM>();
    public ICollection<ECO> ECOs { get; set; } = new List<ECO>();
}
