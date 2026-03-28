using System.ComponentModel.DataAnnotations;

namespace ClientEcommerce.API.DTOs
{
    public class UpdateCategoryDto
    {
        [Required]
        public string Name { get; set; }

        public int? ParentCategoryId { get; set; }
        public bool IsActive { get; set; }

        public IFormFile? Image { get; set; }   // 🔥 ADD
        public bool RemoveImage { get; set; }   // 🔥 ADD (for deleting photo)
    }
}