using System.ComponentModel.DataAnnotations.Schema;

namespace ClientEcommerce.API.Models
{
    [Table("ProductImages")]
    public class ProductImage
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string ImageUrl { get; set; } = null!;

        public bool IsPrimary { get; set; } = false;
    }
}
