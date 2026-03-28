namespace ClientEcommerce.API.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public bool IsActive { get; set; }
        public string Slug { get; set; }
        public int? ParentCategoryId { get; set; }   // 🔥 ADD THIS
        public string? ImageUrl { get; set; }
        public List<CategoryDto> SubCategories { get; set; } = new();
        
    }
}