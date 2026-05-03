using System.ComponentModel.DataAnnotations;

public class AddToCartDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "ProductVariantId must be positive")]
    public int ProductVariantId { get; set; }

    [Required]
    [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
    public int Quantity { get; set; }
}
