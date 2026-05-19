public class OrderItemDetailDto
{
    public string ProductName { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string? Style { get; set; }
    public string? Material { get; set; }
    public string? Color { get; set; }
    public string? Class { get; set; }
    public string? ProductCode { get; set; }
    public int Quantity { get; set; }
}
