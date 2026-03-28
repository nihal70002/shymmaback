namespace ClientEcommerce.API.DTOs
{
    public class PlaceOrderDto
    {
        public int SalesExecutiveId { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }

    public class OrderItemDto
    {
        public int ProductVariantId { get; set; }
        public int Quantity { get; set; }
    }
}
