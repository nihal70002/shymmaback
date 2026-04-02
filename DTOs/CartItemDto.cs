public class CartItemDto
{
    public int ProductId { get; set; }

    public int ProductVariantId { get; set; }

    public required string ProductName { get; set; }

    public string? Size { get; set; }

    public string? Material { get; set; }   // ✅ ADD THIS

    public string? Class { get; set; }      // ✅ ADD THIS

    public string? Color { get; set; }      // ✅ ADD THIS

    public string? ProductCode { get; set; } // ✅ ADD THIS

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public required string ImageUrl { get; set; }
}