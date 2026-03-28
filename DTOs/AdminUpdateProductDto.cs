namespace ClientEcommerce.API.DTOs
{
    public class AdminUpdateProductDto
    {
        public required string Name { get; set; }
        public string? NameArabic { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public required string Description { get; set; }
        public List<string> ImageUrls { get; set; } = [];

    }

}
