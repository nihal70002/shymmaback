using ClientEcommerce.API.DTOs;

namespace ClientEcommerce.API.Services
{
    public interface ICategoryService
    {
        IEnumerable<CategoryDto> GetAll(bool admin);
        void Create(CreateCategoryDto dto, string? imageUrl);
        void Update(int id, UpdateCategoryDto dto, string? imageUrl);
        void Delete(int id);
        CategoryDto? GetCategoryWithChildren(string slug);
    }
}
