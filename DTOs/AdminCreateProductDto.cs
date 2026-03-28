namespace ClientEcommerce.API.DTOs
{
    public class AdminCreateProductDto
    {
        public required string Name { get; set; }
        public string? NameArabic { get; set; }
        public int CategoryId { get; set; }   // ✅ MUST BE int
        
        public int BrandId { get; set; }
        public required string Description { get; set; }
        public List<string> ImageUrls { get; set; } = [];


        // Initializing with an empty list ensures the API doesn't crash 
        // if no variants are sent in the initial request object.
        public List<AdminCreateProductVariantDto> Variants { get; set; } = [];
    }
}