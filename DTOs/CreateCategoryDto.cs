using System.ComponentModel.DataAnnotations;

namespace ClientEcommerce.API.DTOs
{
    public class CreateCategoryDto
    {
        public string Name { get; set; }
        public int? ParentCategoryId { get; set; }

        public IFormFile? Image { get; set; }   // 🔥 ADD THIS
    }
}