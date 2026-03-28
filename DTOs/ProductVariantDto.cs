public class ProductVariantDto
{
    public int VariantId { get; set; }
    public string Size { get; set; } = null!;

    public string? Class { get; set; }
    public string? Style { get; set; }
    public string? Material { get; set; }
    public string? Color { get; set; }

    public decimal Price { get; set; }
    public int AvailableStock { get; set; }
}