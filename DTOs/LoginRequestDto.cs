using System.ComponentModel.DataAnnotations;

namespace ClientEcommerce.API.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        public required string LoginId { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}