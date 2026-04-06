using ClientEcommerce.API.Enum;

namespace ClientEcommerce.API.DTOs
{
    public class AdminCreateProductDto
    {
        public required string Name { get; set; }
        public string? NameArabic { get; set; }
        public int CategoryId { get; set; }   // ✅ MUST BE int
        public ProductType ProductType { get; set; }
        public int BrandId { get; set; }
        public string? Description { get; set; }
        public List<string> ImageUrls { get; set; } = [];
        public List<string> VideoUrls { get; set; } = [];
        public List<ProductComponentDto> Components { get; set; } = new();

        // Initializing with an empty list ensures the API doesn't crash 
        // if no variants are sent in the initial request object.
        public List<AdminCreateProductVariantDto> Variants { get; set; } = [];
    }
}