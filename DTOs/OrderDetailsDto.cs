public class OrderDetailsDto
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public string Status { get; set; }
    public decimal TotalAmount { get; set; }
    
    // Delivery preferences
    public DateTime? PreferredDeliveryDate { get; set; }
    public string? PreferredDeliveryTime { get; set; }
    public string? DeliveryInstructions { get; set; }
    
    public List<OrderItemDto> Items { get; set; } = new();
}
