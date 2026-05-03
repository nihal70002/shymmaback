using System.ComponentModel.DataAnnotations;

namespace ClientEcommerce.API.DTOs
{
    public class PlaceOrderByCustomerDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one item is required")]
        public List<PlaceOrderItemDto> Items { get; set; } = new();
    }

    public class PlaceOrderItemDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "ProductVariantId must be positive")]
        public int ProductVariantId { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
        public int Quantity { get; set; }
    }
}
