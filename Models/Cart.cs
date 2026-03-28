namespace ClientEcommerce.API.Models
{
    public class Cart
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        // Navigation property: EF Core handles this, 
        // null! tells the compiler it won't be null at runtime.
        public User User { get; set; } = null!;

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}