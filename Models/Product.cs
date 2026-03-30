using System.ComponentModel.DataAnnotations.Schema;
using ClientEcommerce.API.Enum;

namespace ClientEcommerce.API.Models
{
    [Table("Products")]
    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? ProductCode { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public string? NameArabic { get; set; }
        public string? Description { get; set; }
        public ICollection<ProductImage> Images { get; set; } = [];
        public ProductType ProductType { get; set; }

        public bool IsActive { get; set; } = true;
        public int BrandId { get; set; }
        public Brand Brand { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<ProductComponent> Components { get; set; }
        public ICollection<ProductVariant> Variants { get; set; } = [];
    }
}
