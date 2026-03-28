namespace ClientEcommerce.API.DTOs
{
    public class UserSummaryDto
    {
        public int UserId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }

        // ✅ ADD THESE
        public string? CompanyName { get; set; }
        public string? PhoneNumber { get; set; }

        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }


    // Detailed info including full order history
    public class UserDetailsDto : UserSummaryDto
    {
        public DateTime JoinDate { get; set; }
        public List<OrderHistoryDto> OrderHistory { get; set; } = [];
    }

    public class OrderHistoryDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public required string Status { get; set; }
        public decimal TotalAmount { get; set; }
    }
}