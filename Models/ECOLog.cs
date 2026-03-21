using System.ComponentModel.DataAnnotations;

namespace PLM.Models
{
    public class ECOLog
    {
        public int Id { get; set; }
        public int ECOId { get; set; }
        public ECO? ECO { get; set; }
        
        [StringLength(100)]
        public string Action { get; set; } = string.Empty; // e.g., "Created", "Approved", "Validated"
        
        public string UserId { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Comments { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
