using Microsoft.EntityFrameworkCore;
using ClientEcommerce.API.Data;
using ClientEcommerce.API.DTOs.Reports;

public class AdminReportService : IAdminReportService
{
    private readonly AppDbContext _context;

    public AdminReportService(AppDbContext context)
    {
        _context = context;
    }

    // 1️⃣ SALES SUMMARY
    public SalesSummaryDto GetSalesSummary()
    {
        var deliveredOrders = _context.Orders
            .Where(o => o.Status == "Delivered");

        var pendingOrders = _context.Orders
            .Where(o => o.Status != "Delivered" && o.Status != "Cancelled");

        var totalRevenue = deliveredOrders.Sum(o => o.TotalAmount);
        var pendingRevenue = pendingOrders.Sum(o => o.TotalAmount);

        var deliveredCount = deliveredOrders.Count();

        return new SalesSummaryDto
        {
            TotalRevenue = totalRevenue,
            PendingRevenue = pendingRevenue,
            AverageOrderValue = deliveredCount == 0 ? 0 : totalRevenue / deliveredCount
        };
    }

    // 2️⃣ MONTHLY SALES
    public IEnumerable<SalesTrendDto> GetMonthlySales(int year)
    {
        return _context.Orders
            .Where(o => o.Status == "Delivered" && o.OrderDate.Year == year)
            .GroupBy(o => o.OrderDate.Month)
            .Select(g => new SalesTrendDto
            {
                Period = g.Key.ToString(), // Month number
                Revenue = g.Sum(x => x.TotalAmount)
            })
            .OrderBy(x => x.Period)
            .ToList();
    }

    // 3️⃣ TOP SELLING PRODUCTS
    public IEnumerable<TopProductDto> GetTopProducts(int top = 5)
    {
        // 1️⃣ DB-safe aggregation (SQL)
        var data = _context.OrderItems
            .AsNoTracking()
            .Where(oi => oi.Order.Status == "Delivered")
            .Select(oi => new
            {
                ProductId = oi.ProductVariant.Product.Id,
                ProductName = oi.ProductVariant.Product.Name,
                Quantity = oi.Quantity,
                Revenue = oi.Quantity * oi.UnitPrice
            })
            .GroupBy(x => new { x.ProductId, x.ProductName })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.ProductName,
                QuantitySold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.Revenue)
            })
            .OrderByDescending(x => x.QuantitySold)
            .Take(top)
            .ToList(); // ✅ materialize here

        // 2️⃣ Fetch primary images separately (safe)
        var productIds = data.Select(x => x.ProductId).ToList();

        var imageMap = _context.ProductImages
            .AsNoTracking()
            .Where(i => productIds.Contains(i.ProductId))
            .GroupBy(i => i.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                ImageUrl = g
                    .OrderByDescending(i => i.IsPrimary)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault()
            })
            .ToDictionary(x => x.ProductId, x => x.ImageUrl);

        // 3️⃣ Final projection
        return data.Select(x => new TopProductDto
        {
            ProductName = x.ProductName,
            ImageUrl = imageMap.GetValueOrDefault(x.ProductId),
            QuantitySold = x.QuantitySold,
            Revenue = x.Revenue
        }).ToList();
    }



    public IEnumerable<CustomerProductInterestDto> GetCustomerProductInterest(int userId)
    {
        return _context.OrderItems
            .Where(oi => oi.Order.Status == "Delivered" &&
                         oi.Order.UserId == userId)
            .GroupBy(oi => oi.ProductVariant.Product.Name)
            .Select(g => new CustomerProductInterestDto
            {
                ProductName = g.Key,
                QuantityBought = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
            })
            .OrderByDescending(x => x.QuantityBought)
            .ToList();
    }
    public IEnumerable<TopCustomerDto> GetTopCustomers(int top = 5)
    {
        return _context.Orders
            .Where(o => o.Status == "Delivered")
            .GroupBy(o => o.User)
            .Select(g => new TopCustomerDto
            {
                UserId = g.Key.Id,
                CustomerName = g.Key.Name,
                Email = g.Key.Email,
                OrdersCount = g.Count(),
                TotalSpent = g.Sum(o => o.TotalAmount)
            })
            .OrderByDescending(x => x.TotalSpent)
            .Take(top)
            .ToList();
    }




}

