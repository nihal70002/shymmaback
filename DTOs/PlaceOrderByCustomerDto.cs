namespace ClientEcommerce.API.DTOs
{
    public class PlaceOrderByCustomerDto
    {
        public List<PlaceOrderItemDto> Items { get; set; } = new();
    }

    public class PlaceOrderItemDto
    {
        public int ProductVariantId { get; set; }
        public int Quantity { get; set; }
    }
}
