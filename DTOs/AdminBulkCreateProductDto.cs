
using ClientEcommerce.API.Enum;
namespace ClientEcommerce.API.DTOs
{
    public class AdminBulkCreateProductDto
    {
        public string? Name { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public ProductType ProductType { get; set; }
        public string? Description { get; set; }
        public List<string> ImageUrls { get; set; } = [];
        public List<string> VideoUrls { get; set; } = [];
        public List<ProductComponentDto> Components { get; set; } = new();
        // IMPORTANT: initialized
        public List<AdminCreateVariantDto> Variants { get; set; } = new();
    }
}
