using ClientEcommerce.API.Models;

public class ProductComponent
{
    public int Id { get; set; }

    public string? CatNo { get; set; }

    public string? InstrumentName { get; set; }

    public int Units { get; set; }

    public int ProductId { get; set; }

    public Product Product { get; set; }
}