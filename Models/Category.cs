using ClientEcommerce.API.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class Category
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public required string Name { get; set; }

    public string? Slug { get; set; }

    public bool IsActive { get; set; } = true;

    public string? ImageUrl { get; set; }

    public int DisplayOrder { get; set; }

    public int? ParentCategoryId { get; set; }

    [ForeignKey("ParentCategoryId")]
    [JsonIgnore]   // ⭐ prevents infinite serialization loop
    public Category? ParentCategory { get; set; }

    public ICollection<Category> SubCategories { get; set; } = new List<Category>();

    [JsonIgnore]   // ⭐ also recommended
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
