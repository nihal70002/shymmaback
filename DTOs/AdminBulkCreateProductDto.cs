namespace ClientEcommerce.API.DTOs
{
    public class AdminBulkCreateProductDto
    {
        public string? Name { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public string? Description { get; set; }
        public List<string> ImageUrls { get; set; } = [];

        // IMPORTANT: initialized
        public List<AdminCreateVariantDto> Variants { get; set; } = new();
    }
}
