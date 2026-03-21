namespace PLM.Domain.Entities;

public class BoMComponent
{
    public int Id { get; set; }
    public int BoMVersionId { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string Unit { get; set; } = "pcs";

    // Navigation property
    public BoMVersion BoMVersion { get; set; } = null!;
}
