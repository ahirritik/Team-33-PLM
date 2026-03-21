using System.ComponentModel.DataAnnotations;

namespace PLM.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty; // e.g., "Update", "Insert"
        public string EntityName { get; set; } = string.Empty; // e.g., "Product", "BoM"
        public string RecordId { get; set; } = string.Empty; // PK of the entity
        
        public string OldValues { get; set; } = string.Empty; // JSON
        public string NewValues { get; set; } = string.Empty; // JSON
        
        public string UserId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
