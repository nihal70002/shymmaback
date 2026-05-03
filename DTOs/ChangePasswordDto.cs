using System.ComponentModel.DataAnnotations;

namespace ClientEcommerce.API.DTOs
{
    public class ChangePasswordDto
    {
        [Required]
        [MinLength(6, ErrorMessage = "Current password is required")]
        public required string CurrentPassword { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters")]
        public required string NewPassword { get; set; }
    }
}