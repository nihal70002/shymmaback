namespace ClientEcommerce.API.DTOs
{
    public class ProductDetailDto
    {
        public int ProductId { get; set; }

        public required string Name { get; set; }
        public string ProductCode { get; set; }
        public string? NameArabic { get; set; }
        public int CategoryId { get; set; }        // ✅ FIX
        public required string CategoryName { get; set; }   // ✅ FIX

        public required string Description { get; set; }
        public List<string> ImageUrls { get; set; } = [];
        public string PrimaryImageUrl { get; set; }


        // Initializing the list ensures scannability and prevents null reference errors
        public List<ProductVariantDto> Sizes { get; set; } = [];
    }
}