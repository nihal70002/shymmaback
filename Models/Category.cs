using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientEcommerce.API.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public required string Name { get; set; }

        public string Slug { get; set; }

        public bool IsActive { get; set; } = true;

        public string? ImageUrl { get; set; }

        // 🔥 NEW — Parent Category
        public int? ParentCategoryId { get; set; }

        [ForeignKey("ParentCategoryId")]
        public Category? ParentCategory { get; set; }

        public ICollection<Category> SubCategories { get; set; } = new List<Category>();

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}