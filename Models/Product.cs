using System.ComponentModel.DataAnnotations;

namespace PLM.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        public decimal SalePrice { get; set; }
        public decimal CostPrice { get; set; }
        public string? Attachments { get; set; }
        public string CurrentVersion { get; set; } = "v1";
        public ProductStatus Status { get; set; } = ProductStatus.Active;

        public ICollection<BoM> BoMs { get; set; } = new List<BoM>();
    }
}
