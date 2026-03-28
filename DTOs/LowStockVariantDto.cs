public class LowStockVariantDto
{
    public int VariantId { get; set; }
    public required string ProductName { get; set; }
    public required string Size { get; set; }
    public int Stock { get; set; }
}
