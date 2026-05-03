using System.ComponentModel.DataAnnotations;

namespace ClientEcommerce.API.DTOs
{
    public class CreateBrandDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Brand name is required")]
        public string? BrandName { get; set; }
    }
}