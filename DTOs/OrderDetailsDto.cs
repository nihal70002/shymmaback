public class OrderDetailsDto
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public string Status { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}
