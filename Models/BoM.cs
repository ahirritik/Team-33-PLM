using System.ComponentModel.DataAnnotations;

namespace PLM.Models
{
    public class BoM
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public string Version { get; set; } = "v1";
        public BoMStatus Status { get; set; } = BoMStatus.Active;

        public ICollection<BoMComponent> Components { get; set; } = new List<BoMComponent>();
        public ICollection<BoMOperation> Operations { get; set; } = new List<BoMOperation>();
    }

    public class BoMComponent
    {
        public int Id { get; set; }
        public int BoMId { get; set; }
        public BoM? BoM { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; } 

        public decimal Quantity { get; set; }
    }

    public class BoMOperation
    {
        public int Id { get; set; }
        public int BoMId { get; set; }
        public BoM? BoM { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        public int TimeMinutes { get; set; }
        
        [StringLength(100)]
        public string WorkCenter { get; set; } = string.Empty;
    }
}
