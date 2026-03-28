using ClientEcommerce.API.Models;

public class Brand
{
    public int BrandId { get; set; }

    // Option A: Make it nullable (Quickest fix)
    public string? BrandName { get; set; }

    // Option B: Initialize the Collection (Best practice)
    // This prevents NullReferenceExceptions when adding products
    public ICollection<Product> Products { get; set; } = new List<Product>();
}