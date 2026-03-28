namespace ClientEcommerce.API.DTOs.Admin
{
    public class AdminDashboardSummaryDto
    {
        public int TotalOrders { get; set; }
        public int PlacedOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int DispatchedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }

        public int TodayOrders { get; set; }

        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }

        public int ActiveProducts { get; set; }
        public int OutOfStockVariants { get; set; }
    }
}
