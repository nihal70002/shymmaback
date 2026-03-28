using System.ComponentModel.DataAnnotations.Schema;

namespace ClientEcommerce.API.Models
{
    [Table("ProductVariants")]
    public class ProductVariant
    {
        public int Id { get; set; }

        [Column("ProductId")] // ✅ MATCH DB EXACTLY
        public int ProductId { get; set; }
        public string? Class { get; set; }
        public string? Style { get; set; }
        public string? Material { get; set; }
        public string? Color { get; set; }

        public Product Product { get; set; } = null!;
        public int LowStockThreshold { get; set; } = 10;
        public string? ProductCode { get; set; }


        public required string Size { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }
    }
}
