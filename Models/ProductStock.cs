using ClientEcommerce.API.Models;

public class ProductStock
{
    public int Id { get; set; }
    public int ProductId { get; set; }

    public int TotalStock { get; set; }
    public int ReservedStock { get; set; }

    public int AvailableStock => TotalStock - ReservedStock;

    public Product Product { get; set; } = null!;
}
