namespace PLM.Domain.Entities;

public class BoMOperation
{
    public int Id { get; set; }
    public int BoMVersionId { get; set; }
    public string OperationName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SequenceOrder { get; set; }
    public decimal EstimatedTime { get; set; }
    public string WorkCenter { get; set; } = string.Empty;

    // Navigation property
    public BoMVersion BoMVersion { get; set; } = null!;
}
