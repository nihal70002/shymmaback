using ClientEcommerce.API.Data;
using ClientEcommerce.API.DTOs.Admin;
using ClientEcommerce.API.Enum;

namespace ClientEcommerce.API.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly AppDbContext _context;

        public AdminDashboardService(AppDbContext context)
        {
            _context = context;
        }

        public AdminDashboardSummaryDto GetSummary()
        {
            var today = DateTime.UtcNow.Date;

            return new AdminDashboardSummaryDto
            {
                TotalOrders = _context.Orders.Count(),

                PlacedOrders = _context.Orders.Count(o =>
                    o.Status != nameof(OrderStatus.Cancelled)
                ),

                ConfirmedOrders = _context.Orders.Count(o =>
                    o.Status == nameof(OrderStatus.Confirmed)
                ),

                DispatchedOrders = _context.Orders.Count(o =>
                    o.Status == nameof(OrderStatus.Dispatched)
                ),

                DeliveredOrders = _context.Orders.Count(o =>
                    o.Status == nameof(OrderStatus.Delivered)
                ),

                CancelledOrders = _context.Orders.Count(o =>
                    o.Status == nameof(OrderStatus.Cancelled)
                ),

                TodayOrders = _context.Orders.Count(o =>
                    o.OrderDate.Date == today
                ),

                TotalRevenue = _context.Orders
                    .Where(o => o.Status == nameof(OrderStatus.Delivered))
                    .Sum(o => (decimal?)o.TotalAmount) ?? 0,

                TodayRevenue = _context.Orders
                    .Where(o =>
                        o.Status == nameof(OrderStatus.Delivered) &&
                        o.OrderDate.Date == today)
                    .Sum(o => (decimal?)o.TotalAmount) ?? 0,

                ActiveProducts = _context.Products.Count(p => p.IsActive),

                OutOfStockVariants = _context.ProductVariants.Count(v => v.Stock <= 0)
            };
        }
    }
}
