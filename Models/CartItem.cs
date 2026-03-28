namespace ClientEcommerce.API.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        public int CartId { get; set; }

        // Navigation property: marked as null! because EF Core 
        // will populate this during database operations.
        public Cart Cart { get; set; } = null!;

        public int ProductVariantId { get; set; }

        // Navigation property: marked as null!
        public ProductVariant ProductVariant { get; set; } = null!;

        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}