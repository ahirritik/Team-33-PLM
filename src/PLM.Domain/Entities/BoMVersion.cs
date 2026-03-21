namespace PLM.Domain.Entities;

public class BoMVersion
{
    public int Id { get; set; }
    public int BoMId { get; set; }
    public int VersionNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public string ChangeDescription { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;

    // Navigation properties
    public BoM BoM { get; set; } = null!;
    public ICollection<BoMComponent> Components { get; set; } = new List<BoMComponent>();
    public ICollection<BoMOperation> Operations { get; set; } = new List<BoMOperation>();
}
