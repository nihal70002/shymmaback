namespace ClientEcommerce.API.DTOs
{
    public class CreateProductDto
    {
        public required string Name { get; set; }
        public string ProductCode { get; set; }
        public int CategoryId { get; set; }   // ✅ int

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }
    }
}