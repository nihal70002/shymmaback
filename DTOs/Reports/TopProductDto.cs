public class TopProductDto
{
    public required string ProductName { get; set; }
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}
