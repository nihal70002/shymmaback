public class OrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string ProductImage { get; set; }
    public decimal Subtotal { get; set; }
}
