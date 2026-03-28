namespace ClientEcommerce.API.DTOs
{
    public class CartItemDto
    {
        public int ProductVariantId { get; set; }

        public required string ProductName { get; set; }

        public required string Size { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public required string ImageUrl { get; set; }   // 🔴 ADDED
    }
}