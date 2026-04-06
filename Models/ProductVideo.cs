using System.ComponentModel.DataAnnotations.Schema;

namespace ClientEcommerce.API.Models
{
    [Table("ProductVideos")]
    public class ProductVideo
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string VideoUrl { get; set; } = null!;
    }
}
