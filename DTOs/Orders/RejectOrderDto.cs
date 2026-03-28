using System.ComponentModel.DataAnnotations;

namespace ClientEcommerce.API.DTOs.Orders
{
    public class RejectOrderDto
    {
        [Required]
        public string Reason { get; set; } = null!;
    }
}
