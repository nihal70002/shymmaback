using System.ComponentModel.DataAnnotations;

namespace ClientEcommerce.API.DTOs
{
    public class CreateCustomerByAdminDto
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string CompanyName { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string PhoneNumber { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        [Required]
        public int SalesExecutiveId { get; set; }
    }
}
