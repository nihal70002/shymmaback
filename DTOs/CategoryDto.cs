namespace ClientEcommerce.API.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public bool IsActive { get; set; }
        public string Slug { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        public List<CategoryDto> SubCategories { get; set; } = new();
        public bool HasChildren => SubCategories != null && SubCategories.Any();
    }

    public class ReorderCategoryDto
    {
        public int Id { get; set; }
        public int DisplayOrder { get; set; }
        public int? ParentCategoryId { get; set; }
    }

    public class ReorderCategoriesDto
    {
        public List<ReorderCategoryDto> Categories { get; set; } = new();
    }
}
