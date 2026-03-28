using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ClientEcommerce.API.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string PhoneNumber { get; set; } = null!;

        public string? CompanyName { get; set; } = null!;

        [Required]
        public string Role { get; set; } = "Customer";

        [Required]
        public string PasswordHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

      
        

        public ICollection<Order> OrdersPlaced { get; set; } = new List<Order>();

    }
}
