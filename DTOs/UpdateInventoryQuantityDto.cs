public class UpdateInventoryQuantityDto
{
    public int ProductId { get; set; }

    // Quantity to ADD or REDUCE
    // Example: +10 (add stock), -5 (reduce stock)
    public int QuantityChange { get; set; }

    public string? Reason { get; set; }
}
