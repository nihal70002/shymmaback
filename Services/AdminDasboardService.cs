using ClientEcommerce.API.Data;
using ClientEcommerce.API.DTOs.Admin;
using ClientEcommerce.API.Enum;
using Microsoft.EntityFrameworkCore;

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

            // Single optimized query to get all order stats at once
            var orderStats = _context.Orders
                .AsNoTracking()
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TotalOrders = g.Count(),
                    PlacedOrders = g.Count(o => o.Status != nameof(OrderStatus.Cancelled)),
                    ConfirmedOrders = g.Count(o => o.Status == nameof(OrderStatus.Confirmed)),
                    DispatchedOrders = g.Count(o => o.Status == nameof(OrderStatus.Dispatched)),
                    DeliveredOrders = g.Count(o => o.Status == nameof(OrderStatus.Delivered)),
                    CancelledOrders = g.Count(o => o.Status == nameof(OrderStatus.Cancelled)),
                    TodayOrders = g.Count(o => o.OrderDate.Date == today),
                    TotalRevenue = g.Where(o => o.Status == nameof(OrderStatus.Delivered)).Sum(o => (decimal?)o.TotalAmount) ?? 0,
                    TodayRevenue = g.Where(o => o.Status == nameof(OrderStatus.Delivered) && o.OrderDate.Date == today).Sum(o => (decimal?)o.TotalAmount) ?? 0
                })
                .FirstOrDefault();

            var activeProducts = _context.Products.Count(p => p.IsActive);
            var outOfStock = _context.ProductVariants.Count(v => v.Stock <= 0);

            return new AdminDashboardSummaryDto
            {
                TotalOrders = orderStats?.TotalOrders ?? 0,
                PlacedOrders = orderStats?.PlacedOrders ?? 0,
                ConfirmedOrders = orderStats?.ConfirmedOrders ?? 0,
                DispatchedOrders = orderStats?.DispatchedOrders ?? 0,
                DeliveredOrders = orderStats?.DeliveredOrders ?? 0,
                CancelledOrders = orderStats?.CancelledOrders ?? 0,
                TodayOrders = orderStats?.TodayOrders ?? 0,
                TotalRevenue = orderStats?.TotalRevenue ?? 0,
                TodayRevenue = orderStats?.TodayRevenue ?? 0,
                ActiveProducts = activeProducts,
                OutOfStockVariants = outOfStock
            };
        }
    }
}
