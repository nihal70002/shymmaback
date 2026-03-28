namespace ClientEcommerce.API.DTOs
{
    public class UserOrderListDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public required string Status { get; set; }
        public decimal TotalAmount { get; set; }
    }
}