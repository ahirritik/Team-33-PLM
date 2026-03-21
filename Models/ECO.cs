using System.ComponentModel.DataAnnotations;

namespace PLM.Models
{
    public class ECO
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ReferenceNumber { get; set; } = string.Empty; // From SEQUENCE
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        public ECOType Type { get; set; }
        
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        
        public int? BoMId { get; set; }
        public BoM? BoM { get; set; }
        
        public string CreatedByUserId { get; set; } = string.Empty;
        
        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
        public bool IsVersionUpdate { get; set; } = true;
        
        public ECOStage Stage { get; set; } = ECOStage.New;
        
        public string ProposedChangesJson { get; set; } = string.Empty; // Storing diff/draft data

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        public ICollection<ECOLog> Logs { get; set; } = new List<ECOLog>();
    }
}
