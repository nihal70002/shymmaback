namespace ClientEcommerce.API.DTOs
{
    public class UpdateCartQuantityDto
    {
        public int ProductVariantId { get; set; }
        public int Quantity { get; set; }
    }
}
