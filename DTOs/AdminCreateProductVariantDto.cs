public class AdminCreateProductVariantDto
{
    public string Size { get; set; }
    public string Class { get; set; }
    public string Style { get; set; }
    public string Material { get; set; }
    public string Color { get; set; }

    public int Stock { get; set; }
    public string? ProductCode { get; set; }
    public decimal Price { get; set; }
}