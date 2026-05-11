namespace ClientEcommerce.API.DTOs
{
    public class UserOrderDetailDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public required string Status { get; set; } // Fixed
        public decimal TotalAmount { get; set; }

        // Delivery preferences
        public DateTime? PreferredDeliveryDate { get; set; }
        public string? PreferredDeliveryTime { get; set; }
        public string? DeliveryInstructions { get; set; }

        public List<UserOrderItemDto> Items { get; set; } = []; // Initialized empty list
    }

    public class UserOrderItemDto
    {
        public required string ProductName { get; set; } // Fixed
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}