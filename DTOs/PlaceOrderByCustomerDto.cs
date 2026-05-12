using System.ComponentModel.DataAnnotations;

namespace ClientEcommerce.API.DTOs
{
    public class PlaceOrderByCustomerDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one item is required")]
        public List<PlaceOrderItemDto> Items { get; set; } = new();

        // Optional delivery preferences
        public DateTime? PreferredDeliveryDate { get; set; }
        
        [StringLength(100, ErrorMessage = "Delivery time must be less than 100 characters")]
        public string? PreferredDeliveryTime { get; set; }
        
        [StringLength(500, ErrorMessage = "Delivery instructions must be less than 500 characters")]
        public string? DeliveryInstructions { get; set; }
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
